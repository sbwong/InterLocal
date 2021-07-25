import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";
import {
	differenceInHours,
	format,
	formatDistanceToNow,
	isThisYear,
} from "date-fns";

import React from "react";
import Typography from "@material-ui/core/Typography";
import VisitProfile from "../pages/Profiles/components/VisitProfile";
import { utcToZonedTime } from "date-fns-tz";

const useStyles = makeStyles((theme: Theme) =>
	createStyles({
		item: {
			marginBottom: 4,
		},
	})
);

export interface PostedByLabelProps {
	author_id: number;
	username: string;
	created_time: string;
	last_edit_time: string;
	is_comment: boolean;
	is_admin: boolean;
}

export default function PostedByLabel({
	author_id,
	username,
	created_time,
	last_edit_time,
	is_comment,
	is_admin,
}: PostedByLabelProps) {
	const classes = useStyles();
	const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
	const postedDate = utcToZonedTime(created_time + "Z", timeZone);
	const today = new Date();
	const isWithinOneDay = differenceInHours(today, postedDate) <= 24;
	const isEdited = last_edit_time !== "0001-01-01T00:00:00" && last_edit_time;

	var formattedDate: string;
	var formattedEditDate: string;

	formattedEditDate = "";

	if (is_comment) {
		formattedDate =
			"on " +
			format(postedDate, "MMMM d, yyyy") +
			" at " +
			format(postedDate, "HH:mm");
		if (isEdited) {
			formattedEditDate =
				"on " +
				format(
					utcToZonedTime(last_edit_time + "Z", timeZone),
					"MMMM d, yyyy"
				);
		}
	} else {
		if (isWithinOneDay) {
			// Display relative time for recent posts (within 24 hours)
			formattedDate = formatDistanceToNow(postedDate) + " ago";
			if (isEdited) {
				formattedEditDate =
					formatDistanceToNow(
						utcToZonedTime(last_edit_time + "Z", timeZone)
					) + " ago";
			}
		} else if (isThisYear(postedDate)) {
			// Display exact date (without year) for posts from this year
			formattedDate = "on " + format(postedDate, "MMMM d");
			if (isEdited) {
				formattedEditDate =
					"on " +
					format(
						utcToZonedTime(last_edit_time + "Z", timeZone),
						"MMMM d"
					);
			}
		} else {
			// Display exact date (with year) for any older posts
			formattedDate = "on " + format(postedDate, "MMMM d, yyyy");
			if (isEdited) {
				formattedEditDate =
					"on " +
					format(
						utcToZonedTime(last_edit_time + "Z", timeZone),
						"MMMM d, yyyy"
					);
			}
		}
	}

	return (
		<div>
			<Typography className={classes.item} variant="caption">
				{" "}
				<b>
					<VisitProfile
						username={username}
						user_id={author_id}
						is_admin={is_admin}
					></VisitProfile>
				</b>{" "}
				{formattedDate}{" "}
				{isEdited && "(Edited " + formattedEditDate + ")"}
			</Typography>
		</div>
	);
}
