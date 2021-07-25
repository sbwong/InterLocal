import PostPreviewBox from "../Posts/components/PostPreviewBox";
import React from "react";
import ReactDOM from "react-dom";
import ReduxProvider from "../../common/ReduxProvider";
import { render } from "@testing-library/react";

let postPreview = (
    <PostPreviewBox
        author_id={32}
        comments={[]}
        content="content"
        content_type="Note"
        created_time="2021-01-01T00:00:00.000000"
        down_count={0}
        is_post={true}
        is_upvote={false}
        last_edit_time="2021-01-02T00:00:00.000000"
        post_id={0}
        tags={["tag1", "tag2"]}
        title="title"
        up_count={0}
        username={"Test"}
    />
);

it("renders without crashing", () => {
    const div = document.createElement("div");

    ReactDOM.render(<ReduxProvider>{postPreview}</ReduxProvider>, div);
});

it("renders expandable post", () => {
    const { post } = render(<ReduxProvider>{postPreview}</ReduxProvider>);
    expect(post).toMatchSnapshot();
});
