using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement.CompilerGenerated;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Impl.Special;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts
{
  internal class RecordPart : SimpleTypePartBase, Class.IClassPart
  {
    public readonly bool CliMutable;

    public RecordPart([NotNull] IFSharpTypeDeclaration declaration, [NotNull] ICacheBuilder cacheBuilder)
      : base(declaration, cacheBuilder) =>
      CliMutable = declaration.Attributes.Any(attr => attr.ShortNameEquals("CLIMutable"));

    public RecordPart(IReader reader) : base(reader) =>
      CliMutable = reader.ReadBool();

    protected override void Write(IWriter writer)
    {
      base.Write(writer);
      writer.WriteBool(CliMutable);
    }

    public override TypeElement CreateTypeElement() =>
      new FSharpClass(this);

    protected override byte SerializationTag =>
      (byte) FSharpPartKind.Record;

    public override MemberDecoration Modifiers
    {
      get
      {
        var modifiers = base.Modifiers;
        modifiers.IsSealed = true;
        return modifiers;
      }
    }

    protected override IList<ITypeMember> GetGeneratedMembers()
    {
      var result = new LocalList<ITypeMember>(base.GetGeneratedMembers());
      if (CliMutable)
        result.Add(new DefaultConstructor(TypeElement));

      if (GetDeclaration() is IRecordDeclaration recordDeclaration)
        result.Add(new FSharpGeneratedConstructorFromFields(this, recordDeclaration.Fields));

      return result.ResultingList();
    }
  }
}
