package main

import (
	"encoding/json"
	"fmt"
	"os"
	"os/exec"
)

type BuildInfo struct {
	Version        string
	Projects_Path  []string
	Nuget_Provider string
}

func main() {
	var data BuildInfo
	b_data, _ := os.ReadFile("Build.json")

	// log file content
	fmt.Println(string(b_data[:]))

	err := json.Unmarshal(b_data, &data)
	if err != nil {
		panic(err)
	}

	// log version
	fmt.Println("Version: ", data.Version)

	fmt.Printf("Projects Path: %s", data.Projects_Path[:])

	for i := 0; i < len(data.Projects_Path); i++ {

		fmt.Printf("Cleaning project...\n\tPath: %s", data.Projects_Path[i])

		clean_cmd := exec.Command("dotnet",
			"clean",
			data.Projects_Path[i])

		output, err := clean_cmd.Output()

		if err != nil {
			fmt.Printf("Build project %s\nError: %s", data.Projects_Path[i], err)
			continue
		}

		fmt.Printf("Output: \t%s", string(output[:]))

		fmt.Printf("Restoring project...\n\tPath: %s", data.Projects_Path[i])

		restore_cmd := exec.Command("dotnet",
			"restore",
			data.Projects_Path[i])

		output, err = restore_cmd.Output()

		if err != nil {
			fmt.Printf("Build project %s\nError: %s", data.Projects_Path[i], err)
			continue
		}

		fmt.Printf("Output: \t%s", string(output[:]))

		fmt.Printf("Building project...\n\tPath: %s", data.Projects_Path[i])

		build_cmd := exec.Command("dotnet",
			"build",
			data.Projects_Path[i],
			"--no-restore",
			"--configuration",
			"release",
			fmt.Sprintf("-p:Version=%s", data.Version))

		output, err = build_cmd.Output()

		if err != nil {
			fmt.Printf("Build project %s\nError: %s", data.Projects_Path[i], err)
			continue
		}

		// log output message
		fmt.Printf("Output: \t%s", string(output[:]))

		fmt.Printf("Publishing project artifacts...\n\tPath: %s", data.Projects_Path[i])

		publish_cmd := exec.Command("dotnet",
			"nuget",
			"push",
			fmt.Sprintf("%s\\bin\\release\\*.nupkg", data.Projects_Path[i]),
			"-s",
			data.Nuget_Provider,
			"--skip-duplicate")

		output, err = publish_cmd.Output()

		if err != nil {
			fmt.Printf("Build project %s\nError: %s", data.Projects_Path[i], err)
			continue
		}

		fmt.Printf("Output: \t%s", string(output[:]))
	}
}
