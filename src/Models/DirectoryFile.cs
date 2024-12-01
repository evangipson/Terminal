namespace Terminal.Models
{
    /// <summary>
    /// An implementation of <see cref="DirectoryEntity"/> for a file.
    /// </summary>
    public class DirectoryFile : DirectoryEntity
    {
        /// <summary>
        /// Renders a file in a console-appropriate manner.
        /// <para>
        /// Files will be rendered as just their <see cref="DirectoryEntity.Name"/>, or their <see cref="DirectoryEntity.Name"/>
        /// and <see cref="DirectoryEntity.Extension"/> joined with a ".", if <see cref="DirectoryEntity.Extension"/> is not
        /// <see langword="null"/> or empty (i.e.: <c>filename.txt</c>).
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see langword="string"/> representation of the file.
        /// </returns>
        public override string ToString() => string.IsNullOrEmpty(Extension) ? Name : string.Join('.', Name, Extension);
    }
}
