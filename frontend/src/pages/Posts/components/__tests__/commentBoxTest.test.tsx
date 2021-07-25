import React from "react";
import ReduxProvider from "../../../../common/ReduxProvider";

import CommentBox from "../commentBox";
import { Comment } from "../../../../slices/postSlice";
import ReactDOM from "react-dom";
import { render } from "@testing-library/react";

const mockComment: Comment = {
    author_id: 0,
	created_time: "2021-04-29T04:13:21.885799",
	down_count: 0,
	is_post: false,
	is_upvote: true,
	up_count: 0,
	username: "swong",
	post_id: 0,
    comment_id: 0,
	last_edit_time: "0001-01-01T00:00:00",
	content_body: "this is a test comment"
}

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<CommentBox comment={mockComment} errorHandler={() => {
                console.log("Error rendering test comment");
            }}/>;
		</ReduxProvider>,
		div
	);
});

it("renders edit profile header correctly", () => {
	const { signup } = render(
		<ReduxProvider>
			<CommentBox comment={mockComment} errorHandler={() => {
                console.log("Error rendering test comment");
            }}/>;
		</ReduxProvider>
	);
	expect(signup).toMatchSnapshot();
});
