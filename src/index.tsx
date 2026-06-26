// type
import type { Dispatch } from "hyperapp"

// import
import { app } from "hyperapp"
import h from "hyperapp-jsx-pragma"

// State
interface State {}

// Action
const action_buttonClick = (state: State) => {
	// effect
	const effect_get = async (dispatch: Dispatch<State>) => {
		const res = await fetch("/api/test?text=hello world");
		alert(await res.text())
	}

	// result
	return [state, effect_get]
}

// Entry Point
addEventListener("load", () => {
	app({
		node: document.body,
		init: {},
		view: (state: State) => (<body>
			<button type="button" onclick={action_buttonClick}>GET</button>
		</body>)
	})
})
