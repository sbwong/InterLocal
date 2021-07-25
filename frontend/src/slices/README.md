# Slices

This folder contains slices of Redux store that allows the application to manage the state of the application.

## Definition of a Slice

A Redux Slice is a collection of reducer logic and actions for a single feature of our app. The name “slice” comes from the idea that we’re splitting up the root Redux state object into multiple “slices” of state.

Relevant resources:

[Documentation of Create Slice](https://redux-toolkit.js.org/api/createSlice)

[Redux in React.js — Reducers and Slices](https://medium.com/swlh/redux-in-react-js-reducers-and-slices-bafafec781e3#:~:text=A%20Redux%20Slice%20is%20a,multiple%20%E2%80%9Cslices%E2%80%9D%20of%20slate)

## Folder Structure

`postSlice.tsx` - Contains API calls, actions, and reducers for anything related to posts

`profileSlice.tsx` - Contains API calls, actions, and reducers for anythiing related to user profiles

`__tests__/` - Contains unit testing files
