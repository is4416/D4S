// type
import type { Dispatch } from "hyperapp"

// import
import { app } from "hyperapp"
import h from "hyperapp-jsx-pragma"

// ---------- ---------- ---------- ---------- ----------
// State
// ---------- ---------- ---------- ---------- ----------

interface State {
	json: Record<string, any>
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

		const json = await res.json()

		dispatch((state: State) => ({ ...state, json }))
	}]
}

// ---------- ---------- ---------- ---------- ----------
// action_createDirectoryTreeDiff
// ---------- ---------- ---------- ---------- ----------

const action_createDirectoryTreeDiff = (state: State) => {
	return [state, async (dispatch: Dispatch<State>) => {
		const data = new FormData()

		data.append("path", "./")
		data.append("json", JSON.stringify(state.json))

		const res = await fetch("/api/createDirectoryTreeDiff", {
			method: "POST",
			body  : data
		})
		const json = await res.json()

		dispatch((state: State) => ({ ...state, json }))
	}]
}

// ---------- ---------- ---------- ---------- ----------
// Entry Point
// ---------- ---------- ---------- ---------- ----------

addEventListener("load", () => {
	app({
		node: document.body,
		init: {
			json: {}
		},
		view: (state: State) => (<body>
			<button type="button" onclick={action_hello}>Hello</button>
			<button type="button" onclick={action_startProcess}>Start Process</button>
			<button type="button" onclick={action_saveToFile}>Save To File</button>
			<button type="button" onclick={action_readFile}>Read File (fetch)</button>
			<button type="button" onclick={action_loadFromFile}>Load From File (API)</button>
			<button type="button" onclick={action_createDirectoryTree}>Create Directory Tree</button>
			<button type="button" onclick={action_createDirectoryTreeDiff}>Create Directory Tree Diff</button>
			<pre>{ JSON.stringify(state.json, null, 2) }</pre>
		</body>)
	})
})
