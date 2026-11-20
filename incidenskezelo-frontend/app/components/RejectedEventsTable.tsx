import { IconChevronRight } from "@tabler/icons-react";
import { Fragment, useState } from "react";
import type { IRejectedEvent } from "~/interfaces/IRejectedEvent";
import { formatTime } from "~/lib/format";
import { rejectedReasonLabels, rejectedReasonTextColors, rejectedSourceLabels } from "~/lib/labels";
import { tableCell, tableHeaderCell } from "~/lib/tableClasses";

type RejectedEventsTableProps = {
  rejectedEvents: IRejectedEvent[];
};

function formatPayload(rawPayload: string) {
  try {
    return JSON.stringify(JSON.parse(rawPayload), null, 2);
  } catch {
    return rawPayload;
  }
}

export function RejectedEventsTable({ rejectedEvents }: RejectedEventsTableProps) {
  const [openIds, setOpenIds] = useState<number[]>([]);

  const toggle = (rejectedEventId: number) =>
    setOpenIds((current) =>
      current.includes(rejectedEventId) ? current.filter((openId) => openId !== rejectedEventId) : [...current, rejectedEventId],
    );

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] border-collapse text-[13px]">
        <thead>
          <tr>
            <th className={`${tableHeaderCell} w-[26px]`}>
              <span className="sr-only">Raw payload</span>
            </th>
            <th className={tableHeaderCell}>Received</th>
            <th className={tableHeaderCell}>Source</th>
            <th className={tableHeaderCell}>Reason</th>
            <th className={tableHeaderCell}>Error</th>
          </tr>
        </thead>
        <tbody>
          {rejectedEvents.map((rejected) => {
            const isOpen = openIds.includes(rejected.rejectedEventId);

            return (
              <Fragment key={rejected.rejectedEventId}>
                <tr className="cursor-pointer hover:bg-panel-2" onClick={() => toggle(rejected.rejectedEventId)}>
                  <td className={tableCell}>
                    <button
                      type="button"
                      aria-expanded={isOpen}
                      aria-label={isOpen ? "Hide raw payload" : "Show raw payload"}
                      onClick={(event) => {
                        event.stopPropagation();
                        toggle(rejected.rejectedEventId);
                      }}
                      className="grid text-text-3"
                    >
                      <IconChevronRight
                        size={15}
                        className={`transition-transform motion-reduce:transition-none ${isOpen ? "rotate-90" : ""}`}
                      />
                    </button>
                  </td>
                  <td className={`${tableCell} whitespace-nowrap text-text-2`}>{formatTime(rejected.receivedAtUtc)}</td>
                  <td className={`${tableCell} font-medium whitespace-nowrap`}>{rejectedSourceLabels[rejected.source]}</td>
                  <td className={`${tableCell} font-semibold whitespace-nowrap ${rejectedReasonTextColors[rejected.reason]}`}>
                    {rejectedReasonLabels[rejected.reason]}
                  </td>
                  <td className={tableCell}>{rejected.error}</td>
                </tr>
                {isOpen && (
                  <tr>
                    <td />
                    <td colSpan={4} className="pr-3 pb-3">
                      <p className="mb-1.5 text-[11.5px] text-text-3">Raw payload, as received</p>
                      <pre className="m-0 rounded-md border border-line bg-panel-2 px-3 py-2.5 font-mono text-xs leading-normal break-all whitespace-pre-wrap">
                        {rejected.rawPayload ? formatPayload(rejected.rawPayload) : "(empty)"}
                      </pre>
                    </td>
                  </tr>
                )}
              </Fragment>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
