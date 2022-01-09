namespace RoleMenuPlugin.Types;

public enum RoleMenuCreationState
{
    /// <summary>
    /// The role menu system is waiting for a button to be pressed.
    /// </summary>
    WaitingForButton,
    
    /// <summary>
    /// The role menu system is waiting for a dropdown to be selected.
    /// </summary>
    WaitingForDropdown,
    
    /// <summary>
    /// The role menu system is waiting for a text input.
    /// </summary>
    WaitingForInput,
}