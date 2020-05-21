using BSOA.Column;
using BSOA.Model;
using System;

namespace BSOA.Demo.Model
{
    /// <summary>
    ///  GENERATED: BSOA Table for 'LogicalLocation' entity.
    /// </summary>
    public partial class LogicalLocationTable : Table<LogicalLocation>
    {
        internal SarifLogBsoa Database;

        internal IColumn<string> Name;
        internal IColumn<int> Index;
        internal IColumn<string> FullyQualifiedName;
        internal IColumn<string> DecoratedName;
        internal IColumn<int> ParentIndex;
        internal IColumn<string> Kind;

        public LogicalLocationTable(SarifLogBsoa database) : base()
        {
            Database = database;

            Name = AddColumn(nameof(Name), ColumnFactory.Build<string>());
            Index = AddColumn(nameof(Index), ColumnFactory.Build<int>(-1));
            FullyQualifiedName = AddColumn(nameof(FullyQualifiedName), ColumnFactory.Build<string>());
            DecoratedName = AddColumn(nameof(DecoratedName), ColumnFactory.Build<string>());
            ParentIndex = AddColumn(nameof(ParentIndex), ColumnFactory.Build<int>(-1));
            Kind = AddColumn(nameof(Kind), ColumnFactory.Build<string>());
        }

        public override LogicalLocation Get(int index)
        {
            return (index == -1 ? null : new LogicalLocation(this, index));
        }
    }
}
