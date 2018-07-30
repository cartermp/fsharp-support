namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open System
open System.IO
open JetBrains.ProjectModel
open JetBrains.ReSharper.Feature.Services.Daemon
open JetBrains.ReSharper.FeaturesTestFramework.Daemon
open JetBrains.ReSharper.Plugins.FSharp.ProjectModelBase
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Psi.Files
open JetBrains.ReSharper.Psi.Resolve
open NUnit.Framework

type CSharpResolveTest() =
    inherit TestWithTwoProjects()

    let highlightingManager = HighlightingSettingsManager.Instance

    [<Test>] member x.``Records 01 - Generated members``() = x.DoNamedTest()
    [<Test>] member x.``Records 02 - CliMutable``() = x.DoNamedTest()
    [<Test>] member x.``Records 03 - Override generated members``() = x.DoNamedTest()

    [<Test>] member x.``Exceptions 01 - Empty``() = x.DoNamedTest()
    [<Test>] member x.``Exceptions 02 - Single field``() = x.DoNamedTest()
    [<Test>] member x.``Exceptions 03 - Multiple fields``() = x.DoNamedTest()
    [<Test>] member x.``Exceptions 04 - Protected ctor``() = x.DoNamedTest()

    [<Test>] member x.``Unions 01 - Simple generated members``() = x.DoNamedTest()
    [<Test>] member x.``Unions 02 - Singletons``() = x.DoNamedTest()
    [<Test>] member x.``Unions 03 - Nested types``() = x.DoNamedTest()
    [<Test>] member x.``Unions 04 - Single case with fields``() = x.DoNamedTest()

    [<Test>] member x.``Simple types 01 - Members``() = x.DoNamedTest()

    override x.RelativeTestDataPath = "cache/csharpResolve"

    override x.MainFileExtension = CSharpProjectFileType.CS_EXTENSION
    override x.SecondFileExtension = FSharpProjectFileType.FsExtension

    override x.DoTest(project: IProject, secondProject: IProject) =
        x.Solution.GetPsiServices().Files.CommitAllDocuments()
        x.ExecuteWithGold(fun writer ->
            let projectFile = project.GetAllProjectFiles() |> Seq.exactlyOne
            let sourceFile = projectFile.ToSourceFiles().Single()
            let psiFile = sourceFile.GetPrimaryPsiFile()

            let daemon = TestHighlightingDumper(sourceFile, writer, null, Func<_,_,_,_>(x.ShouldHighlight))
            daemon.DoHighlighting(DaemonProcessKind.VISIBLE_DOCUMENT)
            daemon.Dump()

            let referenceProcessor = RecursiveReferenceProcessor(fun r -> x.ProcessReference(r, writer))
            psiFile.ProcessThisAndDescendants(referenceProcessor)) |> ignore

    member x.ShouldHighlight highlighting sourceFile settings =
        let severity = highlightingManager.GetSeverity(highlighting, sourceFile, x.Solution, settings)
        severity = Severity.ERROR

    member x.ProcessReference(reference: IReference, writer: TextWriter) =
        match reference.Resolve().DeclaredElement with
        | :? IFSharpTypeMember as typeMember -> writer.WriteLine(typeMember.XMLDocId)
        | _ -> ()
