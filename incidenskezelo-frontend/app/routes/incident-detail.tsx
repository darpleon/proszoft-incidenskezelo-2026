import { Tabs, Text } from "@mantine/core";
import { IconMessage } from "@tabler/icons-react";
import { useState } from "react";
import { CommentForm } from "~/components/CommentForm";
import { CommentList } from "~/components/CommentList";
import { IncidentDetailHeader } from "~/components/IncidentDetailHeader";
import { EventsTable } from "~/components/EventsTable";
import { IncidentOverview } from "~/components/IncidentOverview";
import { comments } from "~/data/comments";
import { events } from "~/data/events";
import { incidents } from "~/data/incidents";
import useAuth from "~/hooks/useAuth";
import type { IComment } from "~/interfaces/IComment";
import type { IIncident, IncidentChanges } from "~/interfaces/IIncident";
import type { Route } from "./+types/incident-detail";

function TabCount({ count }: { count: number }) {
  return (
    <span className="grid h-[17px] min-w-[17px] place-items-center rounded-full bg-[#eff2f1] px-[5px] text-[10px] font-semibold text-text-2">
      {count}
    </span>
  );
}

export default function IncidentDetail({ params }: Route.ComponentProps) {
  const { user } = useAuth();
  const [allComments, setAllComments] = useState<IComment[]>(comments);
  const [allIncidents, setAllIncidents] = useState<IIncident[]>(incidents);
  const incident = allIncidents.find((item) => item.incidentId === Number(params.incidentId));

  if (!incident) {
    return (
      <Text size="sm" className="p-6 text-text-3">
        Incident not found.
      </Text>
    );
  }

  const incidentEvents = events
    .filter((event) => event.incidentId === incident.incidentId)
    .sort((a, b) => a.occurredAtUtc.localeCompare(b.occurredAtUtc));

  const incidentComments = allComments
    .filter((comment) => comment.incidentId === incident.incidentId)
    .sort((a, b) => a.createdAtUtc.localeCompare(b.createdAtUtc));

  const addComment = (body: string) => {
    if (!user) return;

    const newComment: IComment = {
      commentId: Date.now(),
      incidentId: incident.incidentId,
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

  const updateIncident = (changes: IncidentChanges) => {
    const now = new Date().toISOString();
    const closedAtUtc = changes.status ? (changes.status === "Closed" ? now : null) : incident.closedAtUtc;
    setAllIncidents((current) =>
      current.map((item) =>
        item.incidentId === incident.incidentId ? { ...item, ...changes, closedAtUtc, updatedAtUtc: now } : item,
      ),
    );
  };

  return (
    <>
      <IncidentDetailHeader incident={incident} events={incidentEvents} onChange={updateIncident} />

      <Tabs key={incident.incidentId} defaultValue="overview" color="#0f717d" className="flex min-h-0 flex-1 flex-col">
        <Tabs.List className="mx-[22px] mt-[22px] max-[680px]:mx-4">
          <Tabs.Tab value="overview">Overview</Tabs.Tab>
          <Tabs.Tab value="events" rightSection={<TabCount count={incidentEvents.length} />}>
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
            <EventsTable events={incidentEvents} />
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
