import SearchResultsPage from "../SearchResultsPage";
import React from "react";
import ReactDOM from "react-dom";
import ReduxProvider from "../../common/ReduxProvider";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<SearchResultsPage />
		</ReduxProvider>,
		div
	);
});

it("renders SearchResults page", () => {
	const { results } = render(
		<ReduxProvider>
			<SearchResultsPage />
		</ReduxProvider>
	);
	expect(results).toMatchSnapshot();
});