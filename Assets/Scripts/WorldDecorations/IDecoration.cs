
using System;

public interface IDecoration
{
    /// <summary>
    /// Test this local map location for a decoration.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="localBlockPosition"></param>
    /// <param name="random"></param>
    /// <returns>true if the decoration was placed here</returns>
    bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random);

}