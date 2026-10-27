import { Tabs } from "@mantine/core";
import { IconFileDescription, IconMessage, IconTimelineEvent } from "@tabler/icons-react";
import { isAxiosError } from "axios";
import { useState } from "react";
import { useRevalidator } from "react-router";
import api from "~/api/api";
import { CommentForm } from "~/components/CommentForm";
import { CommentList } from "~/components/CommentList";
import { EmptyState } from "~/components/EmptyState";
import { EventsTable } from "~/components/EventsTable";
import { FormAlert } from "~/components/FormAlert";
import { IncidentDetailHeader } from "~/components/IncidentDetailHeader";
import { IncidentOverview } from "~/components/IncidentOverview";
import useAuth from "~/hooks/useAuth";
import type { IComment } from "~/interfaces/IComment";
import type { IEvent } from "~/interfaces/IEvent";
import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { IAssignableUser } from "~/interfaces/IUser";
import type { Route } from "./+types/incident-detail";

function TabCount({ count }: { count: number }) {
  return (
    <span className="grid h-[17px] min-w-[17px] place-items-center rounded-full bg-[#eff2f1] px-[5px] text-[10px] font-semibold text-text-2">
      {count}
    </span>
  );
}

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  try {
    const [incident, assignableUsers, events] = await Promise.all([
      api.Incident.getById(params.incidentId),
      api.User.getAssignable(),
      api.Incident.getEvents(params.incidentId),
    ]);
    return { incident: incident.data, assignableUsers: assignableUsers.data, events: events.data, loadError: null };
  } catch (error) {
    const isMissing = isAxiosError(error) && (error.response?.status === 404 || error.response?.status === 400);
    return {
      incident: null,
      assignableUsers: [] as IAssignableUser[],
      events: [] as IEvent[],
      loadError: isMissing ? "Incident not found." : "The incident could not be loaded.",
    };
  }
}

function changeErrorMessage(error: unknown) {
  if (isAxiosError(error) && error.response?.status === 409) {
    return error.response.data?.title === "Revision conflict"
      ? "Someone else changed this incident. The page has been refreshed."
      : "This status change is not allowed.";
  }
  if (isAxiosError(error) && error.response?.status === 400 && typeof error.response.data?.detail === "string") {
    return error.response.data.detail;
  }
  return "The change could not be saved. Please try again.";
}

export default function IncidentDetail({ loaderData }: Route.ComponentProps) {
  const { incident, assignableUsers, events, loadError } = loaderData;
  const { user } = useAuth();
  const revalidator = useRevalidator();
  const [allComments, setAllComments] = useState<IComment[]>([]);
  const [changeError, setChangeError] = useState<{ incidentId: string; message: string } | null>(null);

  if (!incident) {
    return <EmptyState>{loadError}</EmptyState>;
  }

  const incidentComments = allComments
    .filter((comment) => comment.incidentId === incident.id)
    .sort((a, b) => a.createdAtUtc.localeCompare(b.createdAtUtc));

  const saveChange = async (request: () => Promise<unknown>) => {
    setChangeError(null);
    try {
      await request();
    } catch (error) {
      setChangeError({ incidentId: incident.id, message: changeErrorMessage(error) });
    }
    await revalidator.revalidate();
  };

  const changeStatus = (status: IncidentStatus) => {
    if (status === incident.status) return;
    saveChange(() => api.Incident.updateStatus(incident.id, status, incident.revisions.status));
  };

  const changePriority = (priority: IncidentPriority) => {
    if (priority === incident.priority) return;
    saveChange(() => api.Incident.updatePriority(incident.id, priority, incident.revisions.priority));
  };

  const changeAssignee = (assigneeUserId: number | null) => {
    if (assigneeUserId === incident.assigneeUserId) return;
    saveChange(() => api.Incident.updateAssignee(incident.id, assigneeUserId, incident.revisions.assignee));
  };

  const addComment = (body: string) => {
    if (!user) return;

    const newComment: IComment = {
      commentId: Date.now(),
      incidentId: incident.id,
      authorUserId: user.userId,
      authorName: user.name,
      body,
      createdAtUtc: new Date().toISOString(),
      editedAtUtc: null,
    };
    setAllComments((current) => [...current, newComment]);
  };

  const editComment = (commentId: number, body: string) => {
    const editedAtUtc = new Date().toISOString();
    setAllComments((current) =>
      current.map((comment) => (comment.commentId === commentId ? { ...comment, body, editedAtUtc } : comment)),
    );
  };

  return (
    <>
      <IncidentDetailHeader
        incident={incident}
        assignableUsers={assignableUsers}
        onStatusChange={changeStatus}
        onPriorityChange={changePriority}
        onAssigneeChange={changeAssignee}
      />

      {changeError?.incidentId === incident.id && (
        <div className="mx-[22px] mt-4 max-[680px]:mx-4">
          <FormAlert title="Change not saved" message={changeError.message} />
        </div>
      )}

      <Tabs key={incident.id} defaultValue="overview" color="#0f717d" className="flex min-h-0 flex-1 flex-col">
        <Tabs.List className="mx-[22px] mt-[22px] max-[680px]:mx-4">
          <Tabs.Tab value="overview" leftSection={<IconFileDescription size={14} />}>
            Overview
          </Tabs.Tab>
          <Tabs.Tab value="events" leftSection={<IconTimelineEvent size={14} />} rightSection={<TabCount count={events.length} />}>
            Events
          </Tabs.Tab>
          <Tabs.Tab value="comments" leftSection={<IconMessage size={14} />} rightSection={<TabCount count={incidentComments.length} />}>
            Comments
          </Tabs.Tab>
        </Tabs.List>

        <div className="min-h-0 flex-1 overflow-y-auto p-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
          <Tabs.Panel value="overview">
            <IncidentOverview incident={incident} />
          </Tabs.Panel>
          <Tabs.Panel value="events">
            {events.length > 0 ? <EventsTable events={events} /> : <EmptyState>No linked events yet.</EmptyState>}
          </Tabs.Panel>
          <Tabs.Panel value="comments">
            <CommentList comments={incidentComments} currentUserId={user?.userId} onEdit={editComment} />
            <div className="mt-4 border-t border-line pt-4">
              <CommentForm onSubmit={addComment} />
            </div>
          </Tabs.Panel>
        </div>
      </Tabs>
    </>
  );
}
