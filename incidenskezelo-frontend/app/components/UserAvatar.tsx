import { getInitials } from "~/lib/initials";

type UserAvatarProps = {
  name: string;
  size?: "sm" | "md";
  muted?: boolean;
};

const sizeClasses = {
  sm: "size-[21px] text-[9px]",
  md: "size-7 text-[10px]",
};

export function UserAvatar({ name, size = "sm", muted = false }: UserAvatarProps) {
  return (
    <i
      className={`grid shrink-0 place-items-center rounded-full font-bold not-italic ${sizeClasses[size]} ${
        muted ? "bg-line text-text-2" : "bg-petrol-bg text-petrol"
      }`}
    >
      {getInitials(name)}
    </i>
  );
}
