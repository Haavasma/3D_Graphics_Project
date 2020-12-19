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
	//Basic variables
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

	//Build the address
	udpAddr, err := net.ResolveUDPAddr(protocol, port)
	if err != nil {
		fmt.Println("Wrong Address")
		return
	}

	tcpSocket, err := net.Listen("tcp", address+":8081")
	if err != nil {
		os.Exit(1)
	}
	defer tcpSocket.Close()

	fmt.Println("\nReading " + protocol + " from " + udpAddr.String())

	udpConn, err := net.ListenUDP(protocol, udpAddr)
	if err != nil {
		fmt.Println(err)
	}
	defer udpConn.Close()

	go udpreader.ReadUDP(udpConn)

	go udpreader.CheckDCs()

	tcpreader.ReadTCP(tcpSocket)
}
