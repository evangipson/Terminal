namespace Terminal.Models
{
    public class DirectoryFile : DirectoryEntity
    {
        public override string ToString() => string.Join('.', Name, Extension);
    }
}
