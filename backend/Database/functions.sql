CREATE OR REPLACE FUNCTION
getPostContent(pid INTEGER) RETURNS TEXT AS
$$
DECLARE
	content TEXT;
BEGIN
	SELECT question 
	FROM qa_content q INNER JOIN post p ON q.post_id = p.post_id
	WHERE p.post_id = pid
	UNION 
	SELECT content_body
	FROM note_content n INNER JOIN post p ON n.post_id = p.post_id
	WHERE p.post_id = pid 
	INTO content;
	IF content IS NULL THEN
		content = '';
	END IF;
RETURN content;
END;
$$
LANGUAGE plpgsql;


-------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION
getPostContentAndType(pid INTEGER) RETURNS TABLE(content TEXT, type TEXT) AS
$$
DECLARE
	content TEXT;
	type	TEXT;
BEGIN
	SELECT question 
	FROM qa_content q INNER JOIN post p ON q.post_id = p.post_id
	WHERE p.post_id = pid
	INTO content;
	
	IF content IS NOT NULL THEN
		type = 'qa';
		RETURN QUERY SELECT content, type;
	END IF;
	
	SELECT content_body
	FROM note_content n INNER JOIN post p ON n.post_id = p.post_id
	WHERE p.post_id = pid 
	INTO content;
	
	IF content IS NOT NULL THEN
		type = 'note';
	ELSE
		type = '';
		content = '';
	END IF;
	
RETURN QUERY SELECT content, type;
END;
$$
LANGUAGE plpgsql;

--------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION
deleteUser(uid INTEGER, isContentDeleted BOOLEAN) RETURNS VOID AS
$$
DECLARE
	anon_id INTEGER;
BEGIN
	IF isContentDeleted THEN
		DELETE FROM Post
		WHERE author_id = uid;
		
		DELETE FROM Comment
		WHERE author_id = uid;
	ELSE 
		SELECT user_id
		FROM users
		WHERE username = 'ANON'
		INTO anon_id;

		UPDATE Post 
		SET author_id = anon_id
		WHERE author_id = uid;

		UPDATE Comment
		SET author_id = anon_id
		WHERE author_id = uid;
	END IF;
	
	RETURN;
END;
$$
LANGUAGE plpgsql;

--------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION
getUserScore(uid INTEGER) RETURNS INTEGER AS
$$
DECLARE
	post_score INTEGER;
	comment_score INTEGER;
BEGIN
	-- get net post score
	SELECT SUM(up_count) - SUM(down_count) net 
	FROM Post
	WHERE author_id = uid
	GROUP BY author_id
	INTO post_score;
	-- get net comment score
	SELECT SUM(up_count) - SUM(down_count) net 
	FROM Comment
	WHERE author_id = uid
	GROUP BY author_id
	INTO comment_score;
	
	RETURN COALESCE(post_score, 0) + COALESCE(comment_score, 0);
END;
$$
LANGUAGE plpgsql;