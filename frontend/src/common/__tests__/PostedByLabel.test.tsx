import React from "react";
import ReduxProvider from "../../common/ReduxProvider";
import ReactDOM from "react-dom";
import PostedByLabel from "../PostedByLabel";
import { render } from "@testing-library/react";

const props = {
    author_id: 0,
	username: "test",
    created_time: "2021-04-29T04:13:21.885799",
	last_edit_time: "0001-01-01T00:00:00"
}

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<PostedByLabel {...props}/>
		</ReduxProvider>,
		div
	);
});

it("renders postedbylabel", () => {
	const { postedbylabel } = render(
		<ReduxProvider>
			<PostedByLabel {...props} />
		</ReduxProvider>
	);
	expect(postedbylabel).toMatchSnapshot();
});
