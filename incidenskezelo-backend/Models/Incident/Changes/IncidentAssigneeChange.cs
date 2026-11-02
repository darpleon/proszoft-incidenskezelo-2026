namespace incidenskezelo_backend.Models.Incident.Changes;

public class IncidentAssigneeChange : IncidentChange<int?>
{
    private IncidentAssigneeChange() { }

    internal IncidentAssigneeChange(Incident incident, int? oldAssigneeUserId, int? newAssigneeUserId, ChangeContext context)
        : base(incident, oldAssigneeUserId, newAssigneeUserId, context)
    {
    }
}
