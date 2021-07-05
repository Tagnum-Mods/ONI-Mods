using KSerialization;

namespace TagnumElite.AdvancedElectrolyzers
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class IndustrialElectrolyzer : IndustrialElectrolyzerMachine, ISecondaryOutput
    {
        public ConduitType GetSecondaryConduitType()
        {
            return portInfo.conduitType;
        }

        public CellOffset GetSecondaryConduitOffset()
        {
            return portInfo.offset;
        }
    }
}
