// interface
// ---------- ---------- ---------- ---------- ----------
// JsonItemData
// ---------- ---------- ---------- ---------- ----------

interface JsonItemData {
	attributes       : number
	creationTimeUtc  : string
	lastAccessTimeUtc: string
	lastWriteTimeUtc : string
}

// interface
// ---------- ---------- ---------- ---------- ----------
// JsonFileData extends JsonItemData
// ---------- ---------- ---------- ---------- ----------

interface JsonFileData extends JsonItemData {
	size     : number
	extension: string
}

// interface
// ---------- ---------- ---------- ---------- ----------
// JsonDirectoryData extends JsonItemData
// ---------- ---------- ---------- ---------- ----------

interface JsonDirectoryData extends JsonItemData {
	path       ?: string
	files       : Record<string, JsonFileData>
	directories : Record<string, JsonDirectoryData>
}

// abstract class
// ---------- ---------- ---------- ---------- ----------
// JsonItem
// ---------- ---------- ---------- ---------- ----------

abstract class JsonItem {
	// field
	#parent           : JsonDirectory | null
	#name             : string
	#attributes       : number
	#creationTimeUtc  : string
	#lastAccessTimeUtc: string
	#lastWriteTimeUtc : string

	// constructor
	constructor (json: JsonItemData, name: string, parent?: JsonDirectory) {
		this.#parent            = parent ?? null
		this.#name              = name
		this.#attributes        = json.attributes
		this.#creationTimeUtc   = json.creationTimeUtc
		this.#lastAccessTimeUtc = json.lastAccessTimeUtc
		this.#lastWriteTimeUtc  = json.lastWriteTimeUtc
	}

	// property
	get parent(): JsonDirectory | null {
		return this.#parent
	}

	get name(): string {
		return this.#name
	}

	get attributes(): number {
		return this.#attributes
	}

	get creationTimeUtc(): string {
		return this.#creationTimeUtc
	}

	get lastAccessTimeUtc(): string {
		return this.#lastAccessTimeUtc
	}

	get lastWriteTimeUtc(): string {
		return this.#lastWriteTimeUtc
	}

	// method
	getParentDirectories(): JsonDirectory[] {
		const result: JsonDirectory[] = []

		let parent = this.parent
		while (parent) {
			result.push(parent)
			parent = parent.parent
		}

		return result.reverse()
	}

	getPathParts(): string[] {
		return this.getParentDirectories().map(d => d.path ?? d.name).concat(this.name)
	}
}

// class
// ---------- ---------- ---------- ---------- ----------
// JsonFile extends JsonItem
// ---------- ---------- ---------- ---------- ----------

export class JsonFile extends JsonItem {
	// field
	#size     : number
	#extension: string

	// constructor
	constructor (json: JsonFileData, name: string, parent?: JsonDirectory) {
		super(json, name, parent)

		this.#size      = json.size
		this.#extension = json.extension
	}

	// property
	get size(): number {
		return this.#size
	}

	get extension(): string {
		return this.#extension
	}
}

// class
// ---------- ---------- ---------- ---------- ----------
// JsonDirectory extends JsonItem
// ---------- ---------- ---------- ---------- ----------

export class JsonDirectory extends JsonItem {
	// field
	#path       ?: string
	#files       : Record<string, JsonFile>
	#directories : Record<string, JsonDirectory>

	// constructor
	constructor(json: JsonDirectoryData, name: string, parent?: JsonDirectory) {
		super(json, name, parent)

		this.#path        = json.path
		this.#files       = {}
		this.#directories = {}

		// add file
		Object.entries(json.files).forEach(([name, val]) => {
			this.#files[name] = new JsonFile(val, name, this)
		})

		// add directory
		Object.entries(json.directories).forEach(([name, val]) => {
			this.#directories[name] = new JsonDirectory(val, name, this)
		})
	}

	// property

	/**
	 * ルートディレクトリのみ設定される絶対パス
	 */
	get path(): string | undefined {
		return this.#path
	}

	get files(): Readonly<Record<string, JsonFile>> {
		return this.#files
	}

	get directories(): Readonly<Record<string, JsonDirectory>> {
		return this.#directories
	}
}
