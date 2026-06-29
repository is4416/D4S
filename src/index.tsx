// type
import type { Dispatch } from "hyperapp"

// import
import { app } from "hyperapp"
import h from "hyperapp-jsx-pragma"

// ---------- ---------- ---------- ---------- ----------
// State
// ---------- ---------- ---------- ---------- ----------

interface State {
	message: string;
}

// ---------- ---------- ---------- ---------- ----------
// action_hello
// ---------- ---------- ---------- ---------- ----------

const action_hello = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const res = await fetch("/api/hello")
		alert(await res.text())
	}]
}

// ---------- ---------- ---------- ---------- ----------
// action_startProcess
// ---------- ---------- ---------- ---------- ----------

const action_startProcess = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const res = await fetch("/api/startProcess?app=notepad")
		alert(await res.text())
	}]
}

// ---------- ---------- ---------- ---------- ----------
// action_saveToFile
// ---------- ---------- ---------- ---------- ----------

const action_saveToFile = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const data = new FormData()

		data.append("data", "hello world")
		data.append("path", "../test.txt")

		const res = await fetch("/api/saveToFile", {
			method: "POST",
			body  : data
		})

		alert(await res.text())
	}]
}

// ---------- ---------- ---------- ---------- ----------
// action_readFile
// ---------- ---------- ---------- ---------- ----------

const action_readFile = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const res = await fetch("../test.txt")

		alert(await res.text())
	}]
}

// ---------- ---------- ---------- ---------- ----------
// action_loadFromFile
// ---------- ---------- ---------- ---------- ----------

const action_loadFromFile = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const data = new FormData()

		data.append("path", "../test.txt")

		const res = await fetch("/api/loadFromFile", {
			method: "POST",
			body  : data
		})

		alert(await res.text())
	}]
}

// ---------- ---------- ---------- ---------- ----------
// action_createDirectoryTree
// ---------- ---------- ---------- ---------- ----------

const action_createDirectoryTree = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const data = new FormData()

		data.append("path", "./")

		const res = await fetch("/api/createDirectoryTree", {
			method: "POST",
			body  : data
		})

		const message = await res.text()

		dispatch((state: State) => ({ ...state, message}))
	}]
}

// ---------- ---------- ---------- ---------- ----------
// Entry Point
// ---------- ---------- ---------- ---------- ----------

addEventListener("load", () => {
	app({
		node: document.body,
		init: {
			message: ""
		},
		view: (state: State) => (<body>
			<button type="button" onclick={action_hello}>Hello</button>
			<button type="button" onclick={action_startProcess}>StartProcess</button>
			<button type="button" onclick={action_saveToFile}>SaveToFile</button>
			<button type="button" onclick={action_readFile}>ReadFile (fetch)</button>
			<button type="button" onclick={action_loadFromFile}>LoadFromFile (API)</button>
			<button type="button" onclick={action_createDirectoryTree}>CreationDirectoryTree</button>
			<div>{ state.message }</div>
		</body>)
	})
})
