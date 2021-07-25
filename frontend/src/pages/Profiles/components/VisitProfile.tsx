import Link from "@material-ui/core/Link";
import React from "react";
import VerifiedUserIcon from "@material-ui/icons/VerifiedUser";

interface VisitProfileProps {
	user_id: number;
	username: string;
	is_admin: boolean;
}

export default function VisitProfile({
	user_id,
	username,
	is_admin,
}: VisitProfileProps) {
	const linkPath: string = "/Profile/" + user_id;

	return username ? (
		<Link href={linkPath}>
			{username}
			{is_admin && <VerifiedUserIcon fontSize={"inherit"} />}
		</Link>
	) : null;
}
