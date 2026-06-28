// type
import type { Dispatch } from "hyperapp"

// import
import { app } from "hyperapp"
import h from "hyperapp-jsx-pragma"

// State
interface State {
	message: string;
}

// action_button1Click
const action_button1Click = (state: State) => {
	// effect
	const effect_get = async (dispatch: Dispatch<State>) => {
		const res = await fetch("/api/test?text=hello world");
		alert(await res.text())
	}

	// result
	return [state, effect_get]
}

// action_button2Click

const action_button2Click = (state: State) => {
	const effect = async (dispatch: Dispatch<State>) => {
		const data = new FormData()
		data.append("rootPath", ".")
		const res = await fetch("/api/createJsonTree", {
			method: "POST",
			body  : data
		})
		const json = await res.json()

		dispatch((state: State) => ({
			...state,
			message: JSON.stringify(json, null, "")
		}))
	}

	return [state, effect]
}

// Entry Point
addEventListener("load", () => {
	app({
		node: document.body,
		init: {
			message: ""
		},
		view: (state: State) => (<body>
			<button type = "button" onclick = { action_button1Click }>GET</button>
			<button type = "button" onclick = { action_button2Click }>POST</button>
			<div>{ state.message }</div>
		</body>)
	})
})
