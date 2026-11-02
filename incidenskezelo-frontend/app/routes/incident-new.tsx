import { Button, Input, Textarea, TextInput, Title } from "@mantine/core";
import { isAxiosError } from "axios";
import { useState, type FormEvent } from "react";
import { useNavigate, useRevalidator } from "react-router";
import api from "~/api/api";
import { AssigneeMenu } from "~/components/AssigneeMenu";
import { EventPicker } from "~/components/EventPicker";
import { FormAlert } from "~/components/FormAlert";
import { OptionalLabel } from "~/components/OptionalLabel";
import { PriorityMenu } from "~/components/PriorityMenu";
import type { IEvent } from "~/interfaces/IEvent";
import type { IncidentPriority } from "~/interfaces/IIncident";
import type { IAssignableUser } from "~/interfaces/IUser";
import { INCIDENT_DESCRIPTION_MAX_LENGTH, INCIDENT_TITLE_MAX_LENGTH } from "~/lib/validation";
import type { Route } from "./+types/incident-new";

type FieldErrors = { title?: string; description?: string };

export function meta({}: Route.MetaArgs) {
  return [{ title: "New incident | Incident Manager" }];
}

export async function clientLoader() {
  const [events, assignableUsers] = await Promise.allSettled([api.Event.getLatest(), api.User.getAssignable()]);

  return {
    events: events.status === "fulfilled" ? events.value.data.filter((event) => event.incidentId === null) : ([] as IEvent[]),
    eventsLoadError: events.status === "rejected",
    assignableUsers: assignableUsers.status === "fulfilled" ? assignableUsers.value.data : ([] as IAssignableUser[]),
  };
}

function createErrorMessage(error: unknown) {
  if (isAxiosError(error) && error.response?.status === 400 && typeof error.response.data?.detail === "string") {
    return error.response.data.detail;
  }
  return "The incident could not be saved. Please try again.";
}

export default function IncidentNew({ loaderData }: Route.ComponentProps) {
  const { events, eventsLoadError, assignableUsers } = loaderData;
  const navigate = useNavigate();
  const revalidator = useRevalidator();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<IncidentPriority>("Medium");
  const [assigneeUserId, setAssigneeUserId] = useState<number | null>(null);
  const [eventIds, setEventIds] = useState<number[]>([]);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const selectedEventIds = eventIds.filter((eventId) => events.some((event) => event.eventId === eventId));
  const assigneeName = assignableUsers.find((user) => user.userId === assigneeUserId)?.name ?? null;

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    const errors: FieldErrors = {
      title: title.trim() ? undefined : "Title is required.",
      description: description.trim() ? undefined : "Description is required.",
    };
    setFieldErrors(errors);
    if (Object.values(errors).some(Boolean)) return;

    setIsSubmitting(true);
    setFormError(null);
    try {
      const response = await api.Incident.create({
        title: title.trim(),
        description: description.trim(),
        priority,
        assigneeUserId,
        eventIds: selectedEventIds,
      });
      await revalidator.revalidate();
      navigate(`/incidents/${response.data.id}`);
    } catch (error) {
      setFormError(createErrorMessage(error));
      setIsSubmitting(false);
      revalidator.revalidate();
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate className="flex min-h-0 flex-1 flex-col overflow-y-auto p-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
      {formError && <FormAlert title="Incident not created" message={formError} onClose={() => setFormError(null)} />}

      <Title order={1} className="text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
        New incident
      </Title>

      <div className="mt-5 grid min-h-0 flex-1 grid-cols-[minmax(0,1fr)_minmax(0,1.1fr)] grid-rows-[minmax(0,1fr)] gap-8 max-[1080px]:flex-none max-[1080px]:grid-cols-1 max-[1080px]:grid-rows-none max-[1080px]:gap-5">
        <div className="flex min-w-0 flex-col gap-4">
          <TextInput
            label="Title"
            placeholder="Short summary, e.g. Orders API returns 500 errors"
            value={title}
            onChange={(event) => {
              setTitle(event.currentTarget.value);
              setFieldErrors((current) => ({ ...current, title: undefined }));
            }}
            error={fieldErrors.title}
            maxLength={INCIDENT_TITLE_MAX_LENGTH}
            size="md"
          />
          <Textarea
            label="Description"
            placeholder="What is happening, which services are affected, what you checked so far"
            value={description}
            onChange={(event) => {
              setDescription(event.currentTarget.value);
              setFieldErrors((current) => ({ ...current, description: undefined }));
            }}
            error={fieldErrors.description}
            maxLength={INCIDENT_DESCRIPTION_MAX_LENGTH}
            autosize
            minRows={6}
            maxRows={14}
          />
          <div className="grid grid-cols-2 gap-3 max-[680px]:grid-cols-1">
            <Input.Wrapper label="Priority">
              <PriorityMenu priority={priority} onChange={setPriority} variant="field" />
            </Input.Wrapper>
            <Input.Wrapper label={<OptionalLabel>Assignee</OptionalLabel>}>
              <AssigneeMenu
                users={assignableUsers}
                assigneeUserId={assigneeUserId}
                assigneeName={assigneeName}
                onChange={setAssigneeUserId}
                variant="field"
              />
            </Input.Wrapper>
          </div>
        </div>

        <EventPicker events={events} selectedIds={selectedEventIds} onChange={setEventIds} loadError={eventsLoadError} />
      </div>

      <div className="mt-5 flex justify-end gap-2 border-t border-[#eff2f1] pt-4 max-[680px]:*:flex-1">
        <Button variant="default" onClick={() => navigate("/incidents")} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button type="submit" color="#0f717d" loading={isSubmitting}>
          Create incident
        </Button>
      </div>
    </form>
  );
}
