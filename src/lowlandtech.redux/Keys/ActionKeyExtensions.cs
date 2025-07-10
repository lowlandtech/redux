namespace LowlandTech.Redux.Keys;

internal class ActionKeyExtensions
{
    /// <summary>
    /// Creates a deterministic GUID v5 based on a namespace and a name.
    /// </summary>
    /// <param name="namespaceId">The namespace GUID (e.g., WorkspaceId).</param>
    /// <param name="name">The unique name within the namespace (e.g., entity name).</param>
    /// <returns>A deterministic GUID derived from the namespace and name.</returns>
    public static Guid ToGuidv5(Guid namespaceId, string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        // Convert namespace UUID to bytes (big endian)
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // Compute hash of namespace + name
        byte[] nameBytes = Encoding.UTF8.GetBytes(name);

        byte[] hash;
        using (var sha1 = SHA1.Create())
        {
            sha1.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
            sha1.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
            hash = sha1.Hash!;
        }

        // Most significant bits
        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);

        // Set version to 5 (SHA-1 namespace UUID)
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));

        // Set variant to RFC 4122
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        // Convert back to .NET GUID byte order
        SwapByteOrder(newGuid);

        return new Guid(newGuid);
    }

    /// <summary>
    /// Converts GUID byte array between little-endian and big-endian.
    /// </summary>
    private static void SwapByteOrder(byte[] guid)
    {
        void Swap(int a, int b)
        {
            var t = guid[a];
            guid[a] = guid[b];
            guid[b] = t;
        }

        // Data1
        Swap(0, 3);
        Swap(1, 2);
        // Data2
        Swap(4, 5);
        // Data3
        Swap(6, 7);
    }
}
