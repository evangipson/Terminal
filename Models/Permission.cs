namespace Terminal.Models
{
    /// <summary>
    /// A collection of available permissions for the terminal.
    /// </summary>
    public enum Permission
    {
        None,
        UserRead,
        UserWrite,
        UserExecutable,
        UserAll,
        AdminRead,
        AdminWrite,
        AdminExecutable,
        AdminAll
    }
}
