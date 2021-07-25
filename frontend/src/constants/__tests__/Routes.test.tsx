import { routesArray } from "../Routes";

it("does not contain more than one unique route", () => {
	for (var route of routesArray) {
		const path = route.path;
		expect(routesArray.filter((r) => r.path === path).length).toBe(1);
	}
});
