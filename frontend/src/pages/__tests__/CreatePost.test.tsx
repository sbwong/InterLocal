import CreatePost from "../Posts/CreatePost";
import React from "react";
import ReactDOM from "react-dom";
import ReduxProvider from "../../common/ReduxProvider";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<CreatePost />
		</ReduxProvider>,
		div
	);
});

it("renders CreatePost page", () => {
	const { createpost } = render(
		<ReduxProvider>
			<CreatePost />
		</ReduxProvider>
	);
	expect(createpost).toMatchSnapshot();
});
