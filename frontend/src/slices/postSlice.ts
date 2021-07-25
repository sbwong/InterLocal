import { AppThunk, RootState } from "../app/store";
import { PayloadAction, createSlice } from "@reduxjs/toolkit";
import {
	makeAPIDeleteRequest,
	makeAPIGetRequest,
	makeAPIPostRequest,
	makeAPIPutRequest,
} from "../adapters/api";

import { AxiosResponse } from "axios";
import { catchHandler } from "../slices/profileSlice";

export enum PostType {
	Question = "Question",
	Note = "Note",
}

export interface Content {
	author_id: number;
	created_time: string;
	down_count: number;
	is_post: boolean;
	is_upvote: boolean;
	up_count: number;
	username: string;
	post_id: number;
	is_admin: boolean;
}

export interface Post extends Content {
	comments: Comment[];
	content: string;
	content_type: string;
	last_edit_time: string;
	tags: string[];
	title: string;
}

export interface Comment extends Content {
	comment_id: number;
	last_edit_time: string;
	content_body: string;
}

export interface PostRequest {
	content: string;
	post_type: PostType;
	tags: string[];
	title: string;
}

export interface PostState {
	posts: Post[];
	recentPosts: Post[];
	topTags: string[];
	postCount: number;
	params: Record<string, any>;
}

export const initialState: PostState = {
	posts: [],
	recentPosts: [],
	topTags: [],
	postCount: 0,
	params: {},
};
export const initialCommentState: Comment[] = [];

// Helper function. Takes AxiosResponse and returns Post object
export function convertPostResponse(response: AxiosResponse<any>): Post {
	let res = response.data.res;
	console.log("convertPost", res.content_type);
	let post: Post = {
		author_id: res.author_id,
		comments: res.comments,
		content: res.content,
		content_type: res.content_type,
		created_time: res.created_time,
		down_count: res.down_count,
		is_post: true,
		is_upvote: res.is_upvote,
		last_edit_time: res.last_edit_time,
		post_id: res.post_id,
		tags: res.tags,
		title: res.title,
		up_count: res.up_count,
		username: res.username,
		is_admin: res.is_admin,
	};
	return post;
}

export const postSlice = createSlice({
	name: "posts",
	initialState,
	reducers: {
		createPost(state, action: PayloadAction<Post>) {
			// Upvote and downvote count must be 0 at creation.
			let post: Post = action.payload;
			post.down_count = 0;
			post.up_count = 0;
			state.posts.push(post);
		},
		deletePost(state, action: PayloadAction<Number>) {
			// Payload is post pid. Sends delete call with post pid
			const post = state.posts.find(
				(post) => post.post_id === action.payload
			);
			if (post != null) {
				state.posts = state.posts.filter(
					(state_post) => state_post.post_id !== post.post_id
				);
			} else {
				console.log("Could not find post to delete in store");
			}
		},
		downvotePost(state, action: PayloadAction<Number>) {
			// Payload is the post pid. Find the post with the matching pid.
			const post = state.posts.find(
				(post) => post.post_id === action.payload
			);
			if (post != null) {
				post.down_count += 1;
			} else {
				console.log("Could not find post to downvote in store");
			}
		},
		updatePost(state, action: PayloadAction<Partial<Post>>) {
			// Payload is post. Replaces post in store with newly edited post
			const post = state.posts.find(
				(post) => post.post_id === action.payload.post_id
			);
			console.log("post found to update: ", post?.post_id);
			if (post != null) {
				const updatedPost = { ...action.payload, ...post };
				state.posts = state.posts.filter(
					(state_post) => state_post.post_id !== post.post_id
				);
				state.posts.push(updatedPost);
			} else {
				console.log("Could not find post to update in store");
			}
		},
		upvotePost(state, action: PayloadAction<Number>) {
			// Payload is the post pid. Find the post with the matching pid.
			const post = state.posts.find(
				(post) => post.post_id === action.payload
			);
			if (post != null) {
				post.up_count += 1;
			} else {
				console.log("Could not find post to upvote in store");
			}
		},
		fetchPostsSuccess(state, action: PayloadAction<AxiosResponse>) {
			let posts = action.payload.data.posts;
			const newState = { ...state, posts: posts };
			// console.log(posts);
			// console.log("newState", newState);
			return newState;
		},
		fetchSpecificPostSuccess(state, action: PayloadAction<AxiosResponse>) {
			let response = action.payload;
			let newPost = convertPostResponse(response);
			if (!state.posts.find((post) => post.post_id === newPost.post_id)) {
				state.posts.push(newPost);
			}
		},
		updateComment(state, action: PayloadAction<Comment>) {
			const post = state.posts.find((post) =>
				post.comments.some(
					(comment) =>
						comment.comment_id === action.payload.comment_id
				)
			);
			if (post != null) {
				post.comments.filter(
					(comment) =>
						comment.comment_id !== action.payload.comment_id
				);
				post.comments.push(action.payload);
			} else {
				console.log("Could not find comment to update in store");
			}
		},
		deleteComment(state, action: PayloadAction<Number>) {
			const post = state.posts.find((post) =>
				post.comments.some(
					(comment) => comment.comment_id === action.payload
				)
			);
			if (post != null) {
				post.comments.filter(
					(comment) => comment.comment_id !== action.payload
				);
			} else {
				console.log("Could not find comment to delete in store");
			}
		},
		fetchRecentPostsSuccess(state, action: PayloadAction<AxiosResponse>) {
			let recentPosts = action.payload.data.posts;
			const newState = { ...state, recentPosts: recentPosts };
			console.log("recent posts", recentPosts);
			return newState;
		},
		fetchTopTagsSuccess(state, action: PayloadAction<AxiosResponse>) {
			let topTags = action.payload.data.tags;
			const newState = { ...state, topTags: topTags };
			return newState;
		},
		fetchPostCountSuccess(state, action: PayloadAction<AxiosResponse>) {
			let postCount = action.payload.data.total_posts;
			const newState = { ...state, postCount: postCount };
			return newState;
		},
		setParams(state, action: PayloadAction<Record<string, any>>) {
			let params = action.payload;
			console.log("set params", action.payload);
			const newState = { ...state, params: params };
			return newState;
		},
	},
});

// Directly initializes actions
export const {
	createPost,
	deletePost,
	downvotePost,
	updatePost,
	upvotePost,
	fetchPostsSuccess,
	fetchSpecificPostSuccess,
	deleteComment,
	updateComment,
	fetchRecentPostsSuccess,
	fetchTopTagsSuccess,
	fetchPostCountSuccess,
	setParams,
} = postSlice.actions;

// Selectors
export const selectPosts = (state: RootState) => state.posts.posts;

export const selectPost = (post_id: number) => (state: RootState) => {
	return state.posts.posts.find((post) => post.post_id === post_id);
};
export const selectRecentPosts = (state: RootState) => state.posts.recentPosts;

export const selectTopTags = (state: RootState) => state.posts.topTags;

export const selectPostCount = (state: RootState) => state.posts.postCount;

export const selectParams = (state: RootState) => state.posts.params;

// Async call to create post
export const sendCreate = (
	pleaseCreate: PostRequest,
	callback: (post_id: number) => void
): AppThunk => async (dispatch) => {
	try {
		//TODO: backend team will likely update the body for this endpoint soon
		// right now only accepts either note_body or question_body
		var body;
		switch (pleaseCreate.post_type) {
			case PostType.Note:
				body = {
					tags: pleaseCreate.tags,
					title: pleaseCreate.title,
					note_body: pleaseCreate.content,
				};
				break;
			case PostType.Question:
				body = {
					tags: pleaseCreate.tags,
					title: pleaseCreate.title,
					question_body: pleaseCreate.content,
				};
		}
		let response = await makeAPIPostRequest("/CreatePostFunction", body);
		console.log(response.data);
		let post = convertPostResponse(response);
		dispatch(createPost(post));
		callback(post.post_id);
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

// async call to delete a comment
export const sendCommentDeletion = (
	pleaseDelete: Comment,
	postDeleteHandler: () => void
): AppThunk => async (dispatch) => {
	try {
		let response = await makeAPIDeleteRequest("/DeleteCommentFunction", {
			headers: {
				"Content-Type": "application/json",
			},
			withCredentials: true,
			data: {
				comment_id: pleaseDelete.comment_id,
			},
		});
		console.log(response.data);
		dispatch(deleteComment(pleaseDelete.comment_id));
		postDeleteHandler();
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

// Async call to delete post
export const sendDeletion = (
	pleaseDelete: Post,
	callback: () => void
): AppThunk => async (dispatch) => {
	try {
		let response = await makeAPIDeleteRequest("/DeletePostFunction", {
			headers: {
				"Content-Type": "application/json",
			},
			withCredentials: true,
			data: {
				post_id: pleaseDelete.post_id,
			},
		});
		console.log(response.data);
		dispatch(deletePost(pleaseDelete.post_id));
		callback();
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

// Async call to edit post
export const sendUpdate = (
	pleaseEdit: Partial<Post>,
	postEditHandler: () => void
): AppThunk => async (dispatch) => {
	try {
		let response = await makeAPIPutRequest("/post/" + pleaseEdit.post_id, {
			title: pleaseEdit.title,
			content: pleaseEdit.content,
			upvotes: pleaseEdit.up_count,
			downvotes: pleaseEdit.down_count,
			tags: pleaseEdit.tags,
		});
		console.log(response.data);
		dispatch(updatePost(pleaseEdit));
		postEditHandler();
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

export const sendCommentUpdate = (
	pleaseEdit: Comment,
	postEditCommentHandler: () => void
): AppThunk => async (dispatch) => {
	try {
		let response = await makeAPIPutRequest(
			"/comment/" + pleaseEdit.comment_id,
			{
				content_body: pleaseEdit.content_body,
			}
		);
		console.log(response.data);
		dispatch(updateComment(pleaseEdit));
		postEditCommentHandler();
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

// Async call that makes call to api and dispatches action to reducer along with
// results as payload
// Fetches list of posts
export const fetchPosts = (
	limit: number,
	offset: number,
	author_id?: number,
	tag?: string[],
	isVoteOrdering?: boolean,
	user_type?: string,
	content_type?: string,
	date_type?: string
): AppThunk => async (dispatch) => {
	try {
		// Determine if we're fetching a specific user's recent posts or all recent posts
		var params: Record<string, any> = {};
		params["offset"] = offset;
		params["limit"] = limit;

		if (author_id !== undefined) {
			params["author_id"] = author_id;
		}
		if (tag !== undefined) {
			params["tags"] = JSON.stringify(tag);
		}
		if (isVoteOrdering !== undefined) {
			params["isVoteOrdering"] = isVoteOrdering;
		}

		if (user_type !== undefined) {
			params["user_type"] = user_type;
		}

		if (content_type !== undefined) {
			params["content_type"] = content_type;
		}

		if (date_type !== undefined) {
			params["date_type"] = date_type;
		}

		const options = {
			headers: {
				"Content-Type": "application/json",
			},
			params: params,
			withCredentials: true,
		};
		console.log("params", params);
		let response = await makeAPIGetRequest("/post/", options);
		dispatch(fetchPostsSuccess(response));
		dispatch(setParams(params));
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

export const fetchSpecificPost = (post_id: number): AppThunk => async (
	dispatch
) => {
	try {
		let post = await makeAPIGetRequest("/post/" + post_id);
		console.log(post.data);
		dispatch(fetchSpecificPostSuccess(post));
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

export const fetchComments = async (post_id: number): Promise<Comment[]> => {
	try {
		let post = await makeAPIGetRequest("/post/" + post_id + "/comments");
		console.log(post.data.res);
		return post.data.res;
	} catch (e) {
		console.log(e);
		return [];
	}
};

// Async call to create comment
export const createComment = async (
	pleaseCreate: Comment,
	unauthorizedHandler: (e: any) => void
): Promise<Comment | undefined> => {
	try {
		console.log("pleaseCreate: ", pleaseCreate);
		let response = await makeAPIPostRequest("/CreateCommentFunction", {
			content_body: pleaseCreate.content_body,
			post_id: pleaseCreate.post_id,
		});
		console.log(response.data);
		const res: Comment = {
			author_id: pleaseCreate.author_id,
			comment_id: response.data.comment.comment_id,
			content_body: pleaseCreate.content_body,
			created_time: response.data.comment.created_time,
			down_count: 0,
			is_post: false,
			is_upvote: response.data.comment.is_upvote,
			post_id: pleaseCreate.post_id,
			up_count: 0,
			username: pleaseCreate.username,
			last_edit_time: pleaseCreate.last_edit_time,
			is_admin: pleaseCreate.is_admin,
		};
		return res;
	} catch (e) {
		unauthorizedHandler(e);
		return undefined;
	}
};

export const doUpvoteDownvote = async (
	id: Number,
	isPost: boolean,
	isUpvote: boolean,
	unauthorizedHandler: (e: any) => void
): Promise<Boolean | undefined> => {
	try {
		console.log("upDown: ", isUpvote);
		let response;
		if (isPost) {
			console.log("upDown post id: ", id);
			response = await makeAPIPostRequest("/UpvotePostFunction", {
				post_id: id,
				is_upvote: isUpvote,
			});
		} else {
			console.log("upDown comment id: ", id);
			response = await makeAPIPostRequest("/UpvoteCommentFunction", {
				comment_id: id,
				is_upvote: isUpvote,
			});
		}
		console.log(response.data);
		return true;
	} catch (e) {
		console.log(e);
		unauthorizedHandler(e);
		return undefined;
	}
};

export const cancelUpvoteDownvote = async (
	id: Number,
	isPost: boolean,
	isUpvote: boolean,
	unauthorizedHandler: (e: any) => void
): Promise<Boolean | undefined> => {
	try {
		console.log("Cancel upDown: ", isUpvote);
		let response;
		if (isPost) {
			console.log("Cancel upDown post id: ", id);
			response = await makeAPIPutRequest("/CancelPostVoteFunction", {
				post_id: id,
				is_upvote: isUpvote,
			});
		} else {
			console.log("Cancel upDown comment id: ", id);
			response = await makeAPIPutRequest("/CancelCommentVoteFunction", {
				comment_id: id,
				is_upvote: isUpvote,
			});
		}
		console.log(response.data);
		return true;
	} catch (e) {
		console.log(e);
		unauthorizedHandler(e);
		return undefined;
	}
};

// ******************************* View one post and return it process *******************************
export async function fetchPost(post_id: number) {
	try {
		let response = await makeAPIGetRequest("/post/" + post_id);

		let fetchedPost = convertPostResponse(response);
		console.log(
			"[postSlice - fetchPost()]: post_id: " + fetchedPost.post_id
		);
		console.log(
			"[postSlice - fetchPost()]: post title: " + fetchedPost.title
		);
		return fetchedPost;
	} catch (e) {
		console.log(e);
	}
}

export async function fetchAllPostCount() {
	try {
		var params = {};

		const options = {
			headers: {
				"Content-Type": "application/json",
			},
			params: params,
			withCredentials: true,
		};

		console.log("Get Post Count", params);
		const response = await makeAPIGetRequest(
			"/CountPostsFunction",
			options
		);

		console.log(response);
		//dispatch(fetchPostCountSuccess(response));
		return response.data.total_posts;
	} catch (e) {
		console.log(e);
	}
}

export const fetchPostCount = (
	author_id?: number,
	tag?: string[],
	user_type?: string,
	content_type?: string,
	date_type?: string
): AppThunk => async (dispatch) => {
	try {
		var params: Record<string, any> = {};

		if (author_id !== undefined) {
			params["author_id"] = author_id;
		}

		if (tag !== undefined) {
			params["tags"] = JSON.stringify(tag);
		}

		if (user_type !== undefined) {
			params["user_type"] = user_type;
		}

		if (content_type !== undefined) {
			params["content_type"] = content_type;
		}

		if (date_type !== undefined) {
			params["date_type"] = date_type;
		}

		const options = {
			headers: {
				"Content-Type": "application/json",
			},
			params: params,
			withCredentials: true,
		};

		console.log("Get Post Count", params);
		const response = await makeAPIGetRequest(
			"/CountPostsFunction",
			options
		);

		console.log(response);
		dispatch(fetchPostCountSuccess(response));
		//return response.data.total_posts;
	} catch (e) {
		console.log(e);
	}
};

export const fetchRecentPosts = (limit: number): AppThunk => async (
	dispatch
) => {
	try {
		var params = {
			offset: 0,
			limit: limit,
			isVoteOrdering: false,
		};
		const options = {
			headers: {
				"Content-Type": "application/json",
			},
			params: params,
			withCredentials: true,
		};
		let response = await makeAPIGetRequest("/post/", options);
		dispatch(fetchRecentPostsSuccess(response));
	} catch (e) {
		console.log(e);
	}
};

export const fetchTopTags = (limit: number): AppThunk => async (dispatch) => {
	try {
		var params = {
			limit: limit,
		};
		const options = {
			headers: {
				"Content-Type": "application/json",
			},
			params: params,
			withCredentials: true,
		};
		let response = await makeAPIGetRequest("/GetTopTagsFunction", options);
		dispatch(fetchTopTagsSuccess(response));
	} catch (e) {
		console.log(e);
	}
};

export default postSlice.reducer;
