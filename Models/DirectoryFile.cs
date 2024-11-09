namespace Terminal.Models
{
    /// <summary>
    /// An implementation of <see cref="DirectoryEntity"/> for a file.
    /// </summary>
    public class DirectoryFile : DirectoryEntity
    {
        public override string ToString() => string.IsNullOrEmpty(Extension) ? Name : string.Join('.', Name, Extension);
    }
}
