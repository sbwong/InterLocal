import HelpIcon from "@material-ui/icons/Help";
import React from "react";
import Tooltip from "@material-ui/core/Tooltip";
import Typography from "@material-ui/core/Typography";

export enum FieldPrivacy {
	PRIVATE,
	PUBLIC,
	INITIAL_PRIVATE,
	INITIAL_PUBLIC,
}
export interface HelpTooltipProps {
	title: string;
	privacy: FieldPrivacy;
}

export default function HelpTooltip({ title, privacy }: HelpTooltipProps) {
	let toAppend: string;
	switch (privacy) {
		case FieldPrivacy.PRIVATE:
			toAppend = "This field is private and will never be shared.";
			break;
		case FieldPrivacy.PUBLIC:
			toAppend = "This field is publicly shared.";
			break;
		case FieldPrivacy.INITIAL_PRIVATE:
			toAppend =
				"This field is initially private but can be changed to public later.";
			break;
		case FieldPrivacy.INITIAL_PUBLIC:
			toAppend =
				"This field is initially public but can be changed to private later.";
	}
	return (
		<Tooltip
			title={
				<Typography variant="caption">
					{title + " " + toAppend}
				</Typography>
			}
			placement={"right-start"}
		>
			<HelpIcon />
		</Tooltip>
	);
}
