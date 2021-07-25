import PostFeed from "../pages/Posts/components/PostFeed";
import React from "react";

export default function TagFeed(props: any) {
    const tag = props?.match.params.tag;
    return <PostFeed tag={tag} />;
}
