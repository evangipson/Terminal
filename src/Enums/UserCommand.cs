using Terminal.Services;

namespace Terminal.Enums
{
    /// <summary>
    /// A collection of possible user commands.
    /// <para>
    /// Mirrors all values of <see cref="UserCommandService.GetAllCommands"/>.
    /// </para>
    /// </summary>
    public enum UserCommand
    {
        Exit,
        Help,
        Color,
        Save,
        Commands,
        ListDirectory,
        ChangeDirectory,
        ViewFile,
        MakeFile,
        MakeDirectory,
        EditFile,
        ListHardware,
        ViewPermissions,
        ChangePermissions,
        Date,
        Time,
        Now,
        Network,
        DeleteFile,
        DeleteDirectory,
        Ping,
        MoveFile,
        MoveDirectory,
        MakeUser,
        DeleteUser,
        MakeGroup,
        DeleteGroup,
        AddUserToGroup,
        DeleteUserFromGroup,
        ViewGroup,
        ClearScreen,
        DeleteSave,
        Unknown
    };
}
