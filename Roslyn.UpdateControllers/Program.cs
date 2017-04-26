using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddMethodsToClass
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            var project = workspace.OpenProjectAsync(args[0]).Result;
            var newProject = project;
            var controllersIds = project.Documents.Where(doc => doc.Folders.Any(s => s.Contains("Controllers"))).Select(doc => doc.Id).ToList();
            foreach (var doc in controllersIds)
            {
                var document = newProject.GetDocument(doc);
                SyntaxNode root = document.GetSyntaxRootAsync().Result;
                var controllerDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault() as ClassDeclarationSyntax;
                if (controllerDeclaration == null) continue;

                var baseClassArgumentList = controllerDeclaration.BaseList.DescendantNodes().OfType<TypeArgumentListSyntax>().FirstOrDefault() as TypeArgumentListSyntax;
                if (baseClassArgumentList == null) continue;

                var baseClassName = baseClassArgumentList.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault() as IdentifierNameSyntax;

                if (baseClassName == null) continue;

                var controllerUpdate = new ControllerUpdate(baseClassName.Identifier.ValueText);
                var PostAction = controllerUpdate.PostAllMethod();
                var PatchAction = controllerUpdate.PatchAllMethod();
                var DeleteAction = controllerUpdate.DeleteAllMethod();
                var newControllerDeclaration = controllerDeclaration.AddMembers(PostAction, PatchAction, DeleteAction);

                var newRoot = root.ReplaceNode(controllerDeclaration, newControllerDeclaration);

                // this will format all nodes that have Formatter.Annotation
                newRoot = Formatter.Format(newRoot, Formatter.Annotation, workspace);
                document = document.WithText(newRoot.GetText());
                newProject = document.Project;
            }
            bool changed = workspace.TryApplyChanges(newProject.Solution);
        }
    }
}