import React from "react";
import ReactMarkdown from "react-markdown";
import Typography from "@material-ui/core/Typography";

interface MarkdownProps {
	md: string;
}

function Markdown({ md }: MarkdownProps) {
	return (
		<ReactMarkdown
			components={{
				h1: ({ node, ...props }) => (
					<Typography variant="h4">{props.children}</Typography>
				),
				h2: ({ node, ...props }) => (
					<Typography variant="h5">{props.children}</Typography>
				),
				h3: ({ node, ...props }) => (
					<Typography variant="h6">{props.children}</Typography>
				),
				h4: ({ node, ...props }) => (
					<Typography variant="h6">{props.children}</Typography>
				),
				h5: ({ node, ...props }) => (
					<Typography variant="h6">{props.children}</Typography>
				),
				h6: ({ node, ...props }) => (
					<Typography variant="h6">{props.children}</Typography>
				),
			}}
		>
			{md}
		</ReactMarkdown>
	);
}

export default Markdown;
