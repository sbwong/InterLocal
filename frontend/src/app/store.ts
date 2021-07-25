import { Action, ThunkAction } from "@reduxjs/toolkit";
import { applyMiddleware, combineReducers, createStore } from "redux";
import { persistReducer, persistStore } from "redux-persist";

import postReducer from "../slices/postSlice";
import profileReducer from "../slices/profileSlice";
import storage from "redux-persist/lib/storage";
import thunk from "redux-thunk";

const persistConfig = {
	key: "root",
	storage,
};

// Combine reducers and persist them
const reducers = combineReducers({
	posts: postReducer,
	profile: profileReducer,
});

const persistedReducer = persistReducer(persistConfig, reducers);

// Create a persist store with a middleware
export const store = createStore(persistedReducer, applyMiddleware(thunk));

// Create a persistor for the persistGate
export const persistor = persistStore(store);

export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<
	ReturnType,
	RootState,
	unknown,
	Action<string>
>;
