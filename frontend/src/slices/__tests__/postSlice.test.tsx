import reducer, {
    Post,
    Comment,
    PostState,
    createPost,
    downvotePost,
    updatePost,
    upvotePost,
    createComment,
    updateComment,
    deleteComment,
    deletePost,
    selectPost,
} from '../postSlice';

import {catchHandler} from '../profileSlice';
import { useDispatch } from "react-redux";

import expect from 'expect';

const mockPost: Post = {
    author_id: 1,
    comments: [],
    content: "abc123",
    content_type: "note",
    down_count: 0,
    last_edit_time: "12343",
    post_id: 123,
    created_time: "12343",
    tags: [],
    title: "abcxyz",
    up_count: 0,
    username: "Dwayne Johnson",
    is_upvote: false,
    is_post: true,
};

const mockPostUpdate: Post = {
    author_id: 1,
    comments: [],
    content: "new content",
    content_type: "note",
    down_count: 0,
    last_edit_time: "15000",
    post_id: 123,
    created_time: "12343",
    tags: [],
    title: "new title",
    up_count: 2,
    username: "The Rock",
    is_upvote: false,
    is_post: true,
};

const mockPosts: PostState = {
    posts: [],
    recentPosts: [],
    topTags: [],
    postCount: 0,
    params: {}
}

const mockComment: Comment = {
    author_id: 0,
    comment_id: 0, // will be updated when API request is made
    content_body: "",
    created_time: "",
    last_edit_time: "",
    down_count: 0,
    is_post: false,
    is_upvote: false,
    post_id: 123,
    up_count: 0,
    username: "swong",
}

const mockCommentUpdate: Comment = {
    author_id: 0,
	created_time: "2021-04-29T19:37:14.639532",
	down_count: 0,
	is_post: false,
	is_upvote: true,
	up_count: 0,
	username: "swong",
	post_id: 1047,
    comment_id: 1101,
	//last_edit_time: "2021-04-30T01:59:48.774303",
	content_body: "updated"
}

describe('post slice', () => {
    describe('reducer, actions, selectors', () => {
        it('should create a post with pid 123 and intial 0 for upvotes/downvotes', () => {

            // Act: create a post
            const result = reducer(mockPosts, createPost(mockPost));
        
            expect(result.posts[0].up_count).toEqual(0);
            expect(result.posts[0].down_count).toEqual(0);
            expect(result.posts[0].post_id).toEqual(123);

        });
        it('should upvote and downvote a post appropriately', () => {
            // Act: update a post
            const result = updatePost(mockPostUpdate)
  
            // The updated post should have 2 upvotes
            expect(result.payload.up_count).toEqual(2);

            // Act: upvote a post 
            const upVotePostState: PostState = {
                posts: [mockPost],
                recentPosts: [],
                topTags: [],
                postCount: 0,
                params: {}
            }
            const upvotedPostState = reducer(upVotePostState, upvotePost(123));
            expect(upvotedPostState.posts[0].up_count).toEqual(1)

            const downVotePostState: PostState = {
                posts: [result.payload],
                recentPosts: [],
                topTags: [],
                postCount: 0,
                params: {}
            }
            // Act: downvote a post
            const downvotedPostState = reducer(downVotePostState, downvotePost(123));
            expect(downvotedPostState.posts[0].down_count).toEqual(1)

        });
        it('should edit a post appropriately', () => {
            // Act: update a post
            const result = updatePost(mockPostUpdate)
  
            expect(result.payload.title).toEqual("new title");
            expect(result.payload.content).toEqual("new content");
        });
        it('should delete a post appropriately', () => {
            // Act: delete a post
            const result = deletePost(mockPostUpdate.post_id);
  
            expect(result.payload).toBe(mockPostUpdate.post_id);
        });
    });
  });


  describe('comment slice', () => {
    describe('reducer, actions, selectors', () => {
        it('should create a comment with id 1 on post id 123 ', async () => {
            // const dispatch = useDispatch();

            createComment(mockComment, (e) => console.log("Create comment test error", e)).then(res => {
                console.log("test result", res);
                expect(res?.down_count).toEqual(0);
                expect(res?.up_count).toEqual(0);
                expect(res?.post_id).toEqual(123);
                expect(res?.comment_id).toEqual(1);
            });
            
        })
        
        it('should update a comment with id 1101 on post id 1047 ', async () => {

            const result = updateComment(mockCommentUpdate);

            expect(result.payload.last_edit_time).not.toBe(null);
            expect(result.payload.last_edit_time).not.toBe("0001-01-01T00:00:00");
            expect(result.payload.last_edit_time).not.toBe("");
            expect(result.payload.content_body).not.toBe("this is a test comment");
            expect(result.payload.content_body).toBe("updated");
        })

        it('should delete a comment with id 1101 on post id 1047 ', async () => {

            const result = deleteComment(mockCommentUpdate.comment_id);

            expect(result.payload).toBe(mockCommentUpdate.comment_id);

        })
    });
  });
