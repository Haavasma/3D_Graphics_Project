package main

import (
	"fmt"
	"io/ioutil"
	"net"
	"os"
	"strings"

	"../tcpreader"
	"../udpreader"
)

//yo
func main() {
	// Read address from file
	data, err := ioutil.ReadFile("../data/address")
	if err != nil {
		os.Exit(1)
	}
	address := strings.TrimSpace(string(data))
	port := ":8080"
	if address != "localhost" {
		port = address + port
	}
	protocol := "udp"

	//Build udp address
	udpAddr, err := net.ResolveUDPAddr(protocol, port)
	if err != nil {
		fmt.Println("Wrong Address")
		return
	}

	// set up tcp socket
	tcpSocket, err := net.Listen("tcp", address+":8081")
	if err != nil {
		os.Exit(1)
	}
	defer tcpSocket.Close()

	fmt.Println("\nReading " + protocol + " from " + udpAddr.String())

	// Set up udp connection

	udpConn, err := net.ListenUDP(protocol, udpAddr)
	if err != nil {
		fmt.Println(err)
	}
	defer udpConn.Close()

	defer tcpSocket.Close()

	// read on udp listener in goroutine
	go udpreader.ReadUDP(udpConn)

	// check disconnects in goroutine
	go udpreader.CheckDCs()

	// read from tcp listener in goroutine
	go tcpreader.ReadTCP(tcpSocket)

	// check queue
	tcpreader.CheckQueue()
}
