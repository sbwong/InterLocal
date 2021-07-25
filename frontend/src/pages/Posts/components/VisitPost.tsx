import Link from "@material-ui/core/Link";
import React from "react";

export default function VisitPost(props: any) {
	const linkPath: string = "/Post/" + props.post_id;
	console.log("[VisitPost]: linkPath will be " + linkPath);

	return <Link href={linkPath}></Link>;
}
