using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using ClassDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
using FieldDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Generator used for behaviors.
    /// </summary>
    [Generator]
    public class BehaviorGenerator : ISourceGenerator
    {
        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            /*
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif
            */
        }

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            // Get all the binding contexts
            BindingContextCollection bindingContexts = GetBindingContexts(context, receiver.BindingContexts.Keys);
            Dictionary<INamedTypeSymbol, List<BehaviorData>> behaviorMap = GetBehaviorMap(context, receiver.Behaviors.Keys);
            CreateBehaviorFactories(context, bindingContexts, behaviorMap, receiver.Entities.Keys);
            Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> generatedProperties = CreatePropertyChecks(context, receiver.CheckedFields.Keys);
            CreatePropertyParameterMethods(context, receiver.ParameterSets.Keys, generatedProperties);
        }

        private BindingContextCollection GetBindingContexts(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> @classes)
        {
            BindingContextCollection contexts = new BindingContextCollection();
            foreach (ClassDeclarationSyntax bindingContext in @classes)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(bindingContext.SyntaxTree);
                INamedTypeSymbol symbol = model.GetDeclaredSymbol(bindingContext, context.CancellationToken) as INamedTypeSymbol;

                // Add binding contexts
                foreach (AttributeData attribute in symbol.GetAttributes().Where(attribute => attribute.IsAttribute(Constants.BindingContextFor)))
                {
                    INamedTypeSymbol target = attribute.MakeGenericFromAttribute();
                    contexts.Add(target, symbol);
                }
            }
            return contexts;
        }
        private Dictionary<INamedTypeSymbol, List<BehaviorData>> GetBehaviorMap(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> @classes)
        {
            // First bin all the behaviors based on the entity that they
#pragma warning disable RS1024 // Compare symbols correctly
            Dictionary<INamedTypeSymbol, List<BehaviorData>> behaviorMap = new Dictionary<INamedTypeSymbol, List<BehaviorData>>(SymbolEqualityComparer.Default);
            List<BehaviorData> created = new List<BehaviorData>(8);
            List<INamedTypeSymbol> required = new List<INamedTypeSymbol>(4);
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (ClassDeclarationSyntax behavior in @classes)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(behavior.SyntaxTree);
                INamedTypeSymbol symbol = model.GetDeclaredSymbol(behavior, context.CancellationToken) as INamedTypeSymbol;
                INamedTypeSymbol check = null;
                created.Clear();
                required.Clear();

                // Create the behavior for all entities that were used
                foreach (AttributeData attribute in symbol.GetAttributes())
                {
                    // Deal with BehaviorForAttribute
                    if (attribute.IsAttribute(Constants.BehaviorFor))
                    {
                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol target)
                        {
                            if (!behaviorMap.TryGetValue(target, out List<BehaviorData> behaviorList))
                            {
                                behaviorList = new List<BehaviorData>(8);
                                behaviorMap.Add(target, behaviorList);
                            }

                            BehaviorData data;
                            if (attribute.ConstructorArguments.Length > 1)
                            {
                                data = new BehaviorData(symbol.MakeGeneric(attribute.ConstructorArguments[1]));
                            }
                            else
                            {
                                data = new BehaviorData(symbol);
                            }

                            behaviorList.Add(data);
                            created.Add(data);
                        }
                    }

                    // Deal with AddBehaviorIfNoAttribute
                    if (attribute.IsAttribute(Constants.AddBehaviorIfNo))
                    {
                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol checkItf)
                        {
                            check = checkItf;
                        }
                    }

                    // Deal with BehaviorRequiresAttribute
                    if (attribute.IsAttribute(Constants.BehaviorRequires))
                    {
                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol requiredBehavior)
                        {
                            required.Add(requiredBehavior);
                        }
                    }
                }

                // Update all created behaviors
                INamedTypeSymbol[] arr = required.ToArray();
                foreach (BehaviorData c in created)
                {
                    c.Check = check;
                    c.Required = arr;
                }
            }
            return behaviorMap;
        }
        private void CreateBehaviorFactories(GeneratorExecutionContext context,
            BindingContextCollection bindingContexts,
            Dictionary<INamedTypeSymbol, List<BehaviorData>> behaviorMap,
            IEnumerable<ClassDeclarationSyntax> @classes)
        {
            // Let's start by generating code for incomplete entities
            foreach (ClassDeclarationSyntax entity in @classes)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(entity.SyntaxTree);
                INamedTypeSymbol symbol = model.GetDeclaredSymbol(entity, context.CancellationToken) as INamedTypeSymbol;

                // Get the set of behaviors for this entity
                INamedTypeSymbol bindingContext = bindingContexts.GetBindingContext(symbol);
                BehaviorFactoryResolver factory = new BehaviorFactoryResolver(symbol, behaviorMap[symbol], bindingContext);
                string code = factory.Create();
                context.AddSource(symbol.ToString() + ".Behaviors.cs", code);
            }
        }
        private void CreatePropertyParameterMethods(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> @classes,
            Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> generatedProperties)
        {
            foreach (ClassDeclarationSyntax parameterset in @classes)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(parameterset.SyntaxTree);
                INamedTypeSymbol symbol = model.GetDeclaredSymbol(parameterset, context.CancellationToken) as INamedTypeSymbol;

                ParameterImportExportResolver factory = new ParameterImportExportResolver(symbol, generatedProperties);
                string code = factory.Create();
                context.AddSource($"{symbol}.Named.cs", code);
            }
        }
        private Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> CreatePropertyChecks(GeneratorExecutionContext context, IEnumerable<FieldDeclarationSyntax> fields)
        {
#pragma warning disable RS1024 // Compare symbols correctly
            Dictionary<INamedTypeSymbol, List<(IFieldSymbol, SyntaxTriviaList)>> map = new Dictionary<INamedTypeSymbol, List<(IFieldSymbol, SyntaxTriviaList)>>(SymbolEqualityComparer.Default);
            Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> generated = new Dictionary<INamedTypeSymbol, GeneratedPropertyCollection>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (FieldDeclarationSyntax field in fields)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(field.SyntaxTree);
                foreach (Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax variable in field.Declaration.Variables)
                {
                    IFieldSymbol symbol = model.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
                    INamedTypeSymbol @class = symbol.ContainingType;
                    if (!map.TryGetValue(@class, out List<(IFieldSymbol, SyntaxTriviaList)> list))
                    {
                        list = new List<(IFieldSymbol, SyntaxTriviaList)>();
                        map.Add(@class, list);
                    }
                    list.Add((symbol, field.GetLeadingTrivia()));
                }
            }
            foreach (KeyValuePair<INamedTypeSymbol, List<(IFieldSymbol, SyntaxTriviaList)>> pair in map)
            {
                PropertyResolver factory = new PropertyResolver(pair.Key, pair.Value);
                string code = factory.Create();
                context.AddSource($"{pair.Key.ContainingNamespace}.{pair.Key.Name}.AutoProperties.cs", code);
                generated.Add(pair.Key, factory.Generated);
            }
            return generated;
        }
    }
}
