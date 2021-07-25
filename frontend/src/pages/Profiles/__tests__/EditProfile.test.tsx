import React from "react";
import ReduxProvider from "../../../common/ReduxProvider";
import ReactDOM from "react-dom";
import { EditProfile } from "../EditProfile";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<EditProfile />
		</ReduxProvider>,
		div
	);
});

it("renders EditProfile page", () => {
	const { editprofile } = render(
		<ReduxProvider>
			<EditProfile />
		</ReduxProvider>
	);
	expect(editprofile).toMatchSnapshot();
});
