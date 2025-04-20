using dotPerfStat.Types;

namespace dotPerfStat;

public class MacOS_CPU : IMacCPU
{
    public List<ICPUCore> Cores { get; }
    public ICPUCoreMetadata ArchitectureInformation { get; }
    public MacCPUType ArchitectureFlavor { get; }

    public MacOS_CPU()
    {
        Cores = new List<ICPUCore>();
        ArchitectureInformation = getCPUArchitectureInformation();
        
    }

    private ICPUCoreMetadata getCPUArchitectureInformation()
    {
        
    }
}