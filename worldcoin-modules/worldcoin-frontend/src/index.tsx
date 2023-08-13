import App from "./App";
import Execute from "./components/execute"
import Transfer from "./components/transfer"
import React from "react";
import ReactDOM from "react-dom/client";
import reportWebVitals from "./reportWebVitals";
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";
import Register from "./components/register";
import RegisterSuccess from "./components/registerSuccess";

// @ts-ignore
window.Buffer = Buffer;

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
  },
	{
		path: "/execute",
		element: <Execute />
	},
	{
		path: "/transfer",
		element: <Transfer />
	},
	{
		path: "/register",
		element: <Register />
	},
	{
		path: "/register/success",
		element: <RegisterSuccess />
	},
]);


const root = ReactDOM.createRoot(document.getElementById("root") as HTMLElement);
root.render(
	<React.StrictMode>

		<RouterProvider router={router} />
	</React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
