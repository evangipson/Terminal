namespace Terminal.Models
{
    public class DirectoryFile : DirectoryEntity
    {
        public override string ToString() => string.IsNullOrEmpty(Extension) ? Name : string.Join('.', Name, Extension);
    }
}
