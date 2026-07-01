/**
 * イベントメッセージを拾って、アクションに渡す
 *
 * @param {string} message - イベントメッセージ
 * @param {string} selector - セレクタ
 * @param {(e: Event) => void} fn - アクション
 */
const onMessage = (message, selector, fn) => addEventListener(
	message,
	e => {
		if (e.target.closest(selector)) fn(e)
	}
)


// hello
onMessage("click", "#hello", async () => {
	const res = await fetch("/api/hello")
	alert(await res.text())
})


// executeProcess
onMessage("click", "#executeProcess", async () => {
	const res = await fetch("/api/executeProcess?app=notepad")
	alert(await res.text())
})


// saveToFile
onMessage("submit", "#saveToFile", async e => {
	e.preventDefault()

	const form = e.target

	const res = await fetch("/api/saveToFile", {
		method: form.method,
		body  : new FormData(form)
	});

	alert(await res.text())
})


// loadFromFile
onMessage("click", "#loadFromFile", async e => {
	const data = new FormData()

	data.append("path", "../test.txt")

	const res = await fetch("/api/loadFromFile", {
		method: "POST",
		body  : data
	})

	const pre = document.querySelector("#loadFromFile + pre")
	pre.innerHTML = await res.text()
})


// createDirectoryTree
onMessage("click", "#createDirectoryTree", async e => {
	const data = new FormData()

	data.append("path", "./")

	const res = await fetch("/api/createDirectoryTree", {
		method: "POST",
		body  : data
	})

	const pre = document.querySelector("#createDirectoryTreeDiff + pre")
	pre.textContent = JSON.stringify(await res.json(), null, 2)
})


// createDirectoryTreeDiff
onMessage("click", "#createDirectoryTreeDiff", async e => {
	const pre = document.querySelector("#createDirectoryTreeDiff + pre")

	if (pre.innerHTML == "") {
		alert("初めに CreateDirectoryTree を実行してください")
		return
	}

	const data = new FormData()

	data.append("path", "./")
	data.append("json", pre.innerHTML)

	const res = await fetch("/api/createDirectoryTreeDiff", {
		method: "POST",
		body  : data
	})

	pre.textContent = JSON.stringify(await res.json(), null, 2)

	alert("差分更新")
})


// openFolderDialog
onMessage("click", "#openFolderDialog", async () => {
	const app = "C:\\Users\\isYos\\Desktop\\D4S\\OpenFolderDialog.exe"
	const res = await fetch("/api/executeProcess?app=" + app)
	alert(await res.text())
})
