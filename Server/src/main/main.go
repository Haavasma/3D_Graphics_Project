package main

import (
	"fmt"
	"net"
	"os"

	"../tcpserver"
	"../udpserver"
)

//yo
func main() {
	//Basic variables
	port := ":8080"
	protocol := "udp"

	//Build the address
	udpAddr, err := net.ResolveUDPAddr(protocol, port)
	if err != nil {
		fmt.Println("Wrong Address")
		return
	}

	tcpSocket, err := net.Listen("tcp", "127.0.0.1:8081")
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

	go udpserver.ReadUDP(udpConn)

	tcpserver.ReadTCP(tcpSocket)
}
