import App from "./App";
import { Provider } from "react-redux";
import React from "react";
import { render } from "@testing-library/react";
import { store } from "./app/store";

test("renders learn react link", () => {
	const { app } = render(
		<Provider store={store}>
			<App />
		</Provider>
	);

	expect(app).toMatchSnapshot();
});
