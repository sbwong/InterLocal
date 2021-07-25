CREATE OR REPLACE FUNCTION  
   updatePostRating() RETURNS TRIGGER AS
$$
DECLARE 
BEGIN
	UPDATE Post
	SET 
	up_count = (SELECT COUNT(*) FROM Post_Rating WHERE post_id = new.post_id AND is_upvote),
	down_count = (SELECT COUNT(*) FROM Post_Rating WHERE post_id = new.post_id AND NOT is_upvote)
	WHERE post_id = new.post_id;

   RETURN new;
END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS  updatePostRating ON  Post_Rating;
CREATE TRIGGER Post_Rating 
AFTER INSERT OR UPDATE ON Post_Rating 
FOR EACH ROW
EXECUTE PROCEDURE updatePostRating();
--------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION  
   updateCommentRating() RETURNS TRIGGER AS
$$
DECLARE 
BEGIN
	UPDATE Comment
	SET 
	up_count = (SELECT COUNT(*) FROM Comment_Rating WHERE comment_id = new.comment_id AND is_upvote),
	down_count = (SELECT COUNT(*) FROM Comment_Rating WHERE comment_id = new.comment_id AND NOT is_upvote)
	WHERE comment_id = new.comment_id;
   RETURN new;
END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS  updateCommentRating ON  Comment_Rating;
CREATE TRIGGER updateCommentRating 
AFTER INSERT OR UPDATE ON Comment_Rating 
FOR EACH ROW
EXECUTE PROCEDURE updateCommentRating();

--------------------------------------------------------------------------------
 
CREATE OR REPLACE FUNCTION  
   updateEditTime() RETURNS TRIGGER AS
$$
DECLARE 
BEGIN
	new.last_edit_time := NOW();
   RETURN new;
END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS  updateEditTime ON  Post;
CREATE TRIGGER updateEditTime 
BEFORE UPDATE OF title ON Post 
FOR EACH ROW
EXECUTE PROCEDURE updateEditTime();


DROP TRIGGER IF EXISTS  updateEditTime ON  Comment;
CREATE TRIGGER updateEditTime 
BEFORE UPDATE OF content_body ON Comment 
FOR EACH ROW
EXECUTE PROCEDURE updateEditTime();

--------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION  
   updateEditTimeFromContent() RETURNS TRIGGER AS
$$
DECLARE 
BEGIN
	UPDATE Post SET last_edit_time = NOW() WHERE post_id = old.post_id;
	RETURN new;
END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS  updateEditTimeFromContent ON QA_Content;
CREATE TRIGGER updateEditTimeFromContent 
BEFORE UPDATE OF question ON QA_Content 
FOR EACH ROW
EXECUTE PROCEDURE updateEditTimeFromContent();

DROP TRIGGER IF EXISTS  updateEditTimeFromContent ON Note_Content;
CREATE TRIGGER updateEditTimeFromContent 
BEFORE UPDATE OF content_body ON Note_Content 
FOR EACH ROW
EXECUTE PROCEDURE updateEditTimeFromContent();

--------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION  
   addUserPreferences() RETURNS TRIGGER AS
$$
DECLARE 
BEGIN
	INSERT INTO User_Preferences VALUES (new.user_id);
	return new;
END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS  addUserPreferences ON  Users;
CREATE TRIGGER addUserPreferences 
AFTER INSERT ON Users 
FOR EACH ROW
EXECUTE PROCEDURE addUserPreferences();

--------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION  
   deleteUser() RETURNS TRIGGER AS
$$
DECLARE 
BEGIN
	IF old.username = 'ANON' THEN
		RAISE EXCEPTION 'CANNOT DELETE ANON USER';
	END IF;
	PERFORM deleteUser(old.user_id, up.isContentDeleted) 
	FROM Users u INNER JOIN User_Preferences up ON u.user_id = up.user_id
	WHERE u.user_id = old.user_id;
	
	RETURN old;
END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS  deleteUser ON Users;
CREATE TRIGGER deleteUser 
BEFORE DELETE ON Users 
FOR EACH ROW
EXECUTE PROCEDURE deleteUser();