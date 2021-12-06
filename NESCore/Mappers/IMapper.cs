namespace NESCore.Mappers;

public interface IMapper
{
    /// <summary>
    /// Given an address, it returns the mapped address for the NES to use.
    /// The mapper can execute operations on this address to switch banks, sprites and stuff.
    ///
    /// The NES shouldn't be aware of any of this happening, as the mapper is a physical chip that routes
    /// incoming reads and writes.
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public ushort Map(ushort addr);
}