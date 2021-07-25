CREATE TYPE STATUS_TYPE AS ENUM
(
	'admin',
	'user'
);

CREATE TABLE Users (
	user_id				SERIAL,
	username 			TEXT NOT NULL UNIQUE,
	college 			TEXT NOT NULL,
	email 				TEXT NOT NULL,
	phone_number		TEXT,
	country 			TEXT NOT NULL,
	user_status 		STATUS_TYPE NOT NULL,
	first_name			TEXT NOT NULL,
	last_name			TEXT NOT NULL,
	profile_pic_url 	TEXT,
	year 				TEXT NOT NULL,
	bio					TEXT DEFAULT '',
	PRIMARY KEY (user_id)
);

CREATE TABLE User_Preferences
(
	user_id 		INTEGER NOT NULL,
	isEmailPublic	BOOLEAN DEFAULT FALSE,
	isPhonePublic	BOOLEAN DEFAULT FALSE,
	isCountryPublic	BOOLEAN DEFAULT TRUE,
	isYearPublic	BOOLEAN DEFAULT TRUE, 
	isResidentialCollegePublic BOOLEAN DEFAULT TRUE,
	isContentDeleted BOOLEAN DEFAULT FALSE,
	FOREIGN KEY(user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE Category
(
	category_id		SERIAL PRIMARY KEY,
	category_name	TEXT NOT NULL
);

CREATE TABLE Tag
(
	tag_id		SERIAL PRIMARY KEY,
	tag_name		TEXT NOT NULL
);

CREATE TABLE Post
(
	post_id 		SERIAL,
	author_id		INTEGER NOT NULL, 
	title			TEXT NOT NULL,
	created_time	TIMESTAMP NOT NULL DEFAULT NOW(),
	last_edit_time	TIMESTAMP,
	up_count        INTEGER DEFAULT 0,
    down_count      INTEGER DEFAULT 0,
	PRIMARY KEY (post_id),
	FOREIGN KEY(author_id) REFERENCES Users(user_id)
);

CREATE TABLE Post_Category
(
	post_id			INTEGER NOT NULL,
	category_id 	INTEGER NOT NULL,
	PRIMARY KEY(post_id, category_id),
	FOREIGN KEY(post_id) REFERENCES Post(post_id) ON DELETE CASCADE,
	FOREIGN KEY(category_id) REFERENCES Category(category_id) ON DELETE CASCADE
);

CREATE TABLE Post_Tag
(
	post_id			INTEGER NOT NULL,
	tag_id 			INTEGER NOT NULL,
	PRIMARY KEY(post_id, tag_id),
	FOREIGN KEY(post_id) REFERENCES Post(post_id) ON DELETE CASCADE,
	FOREIGN KEY(tag_id) REFERENCES Tag(tag_id) ON DELETE CASCADE
);

CREATE TABLE Comment
(
	comment_id 		SERIAL,
	post_id 		INTEGER NOT NULL,
	author_id		INTEGER NOT NULL, 
	parent_id		INTEGER,
	content_body	TEXT NOT NULL,
	created_time	TIMESTAMP NOT NULL DEFAULT NOW(),
	last_edit_time	TIMESTAMP,
	up_count        INTEGER DEFAULT 0,
    down_count      INTEGER DEFAULT 0,
	PRIMARY KEY (comment_id),
	FOREIGN KEY (post_id) REFERENCES Post(post_id) ON DELETE CASCADE,
	FOREIGN KEY(author_id) REFERENCES Users(user_id)
);

CREATE TABLE Note_Content
(
	post_id 		INTEGER PRIMARY KEY,
	content_body	TEXT NOT NULL,
	FOREIGN KEY(post_id) REFERENCES Post(post_id) ON DELETE CASCADE
);

CREATE TABLE QA_Content
(
	post_id 		INTEGER PRIMARY KEY,
	question		TEXT NOT NULL,
	answer_id		INTEGER,
	FOREIGN KEY(post_id) REFERENCES Post(post_id) ON DELETE CASCADE,
	FOREIGN KEY(answer_id) REFERENCES Comment(comment_id) ON DELETE CASCADE
);

CREATE TABLE Post_Rating
 (
	post_id 		INTEGER NOT NULL,
	user_id			INTEGER NOT NULL,
	is_upvote		BOOLEAN NOT NULL,
	PRIMARY KEY (post_id, user_id),
	FOREIGN KEY(post_id) REFERENCES Post(post_id) ON DELETE CASCADE,
	FOREIGN KEY(user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE Comment_Rating 
(
	comment_id 		INTEGER NOT NULL,
	user_id			INTEGER NOT NULL,
	is_upvote		BOOLEAN,
	PRIMARY KEY (comment_id, user_id),
	FOREIGN KEY(comment_id) REFERENCES Comment(comment_id) ON DELETE CASCADE,
	FOREIGN KEY(user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE Credentials
(
	user_id 		INTEGER NOT NULL,
	password 		TEXT,
	PRIMARY KEY(user_id),
	FOREIGN KEY(user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);